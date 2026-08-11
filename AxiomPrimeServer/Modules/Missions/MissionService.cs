using AxiomPrime_Metadata.Missions;

public class MissionService : IMissionService
{
    private readonly MissionRepository m_missionRepository;
    private readonly PlayerLockProvider m_playerLockProvider;

    public MissionService(
        MissionRepository missionRepository,
        PlayerLockProvider playerLockProvider)
    {
        m_missionRepository = missionRepository;
        m_playerLockProvider = playerLockProvider;
    }

    public Task<List<Mission_Database>> GetAsync(string playerId)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var missions = await m_missionRepository.GetAsync(playerId);
            foreach(var mission in missions)
                await UpdateMissionStatus(mission);
            return await m_missionRepository.GetAsync(playerId);
        });

    public Task<bool> AddMission(string playerId, Mission_Database mission)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            if (existing.Any(x => x.Identity.Id == mission.Identity.Id))
                return true;

            mission.PlayerId = playerId;
            await m_missionRepository.AddAsync(mission);
            return true;
        });

    public Task<bool> RemoveMission(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if(current == null)
                return false;

            await m_missionRepository.RemoveAsync(current);
            return true;
        });

    public Task<bool> UpdateMissionData(string playerId, Guid missionID, MissionGeneralData generalData)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if(current == null)
                return false;

            current.GeneralData = generalData;
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    /// <summary>
    /// Updates mission reward
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <param name="reward"></param>
    /// <returns></returns>
    public Task<bool> UpdateMissionReward(string playerId,Guid missionID, MissionReward_Database reward)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if(current == null)
                return false;

            current.Reward = reward;
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    /// <summary>
    /// Starts mission travel based on set time in GeneralData.
    /// Half of time is flyingTo state -> then flyingBack state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <param name="shipID"></param>
    /// <returns></returns>
    public Task<bool> StartTravel(string playerId, Guid missionID, Guid shipID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            current.State.ShipID = shipID;
            current.State.CurrentState = MissionState.State.FlyingTo;
            current.State.TimeLeft = current.GeneralData.Time;
            current.State.LastUpdate = DateTime.UtcNow;
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    /// <summary>
    /// Skips all travel and sets mission into finished state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> SkipTraveling(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            if(current.State.CurrentState != MissionState.State.FlyingTo
            && current.State.CurrentState != MissionState.State.FlyingBack)
            return false;

            current.State.CurrentState = MissionState.State.Finished;
            current.State.TimeLeft = 0;
            current.State.SkippedTravel = true;
            
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    /// <summary>
    /// Player aborst mission only if it is flying to destination
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> AbortMission(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            if(current.State.CurrentState != MissionState.State.FlyingTo)
                return false;

            current.State.CurrentState = MissionState.State.FlyingBack;
            current.State.Aborted = true;
            current.State.TimeLeft = current.GeneralData.Time - current.State.TimeLeft;
            current.State.SkippedTravel = false;
            
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    /// <summary>
    /// Checks if mission is in finished state and fight was seen
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> IsMissionFinished(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            await UpdateMissionStatus(current);

            if(current.State.CurrentState == MissionState.State.Finished)
                if(current.State.SeenFight || current.State.Aborted)
                    return true;
                
            return false;
        });

    /// <summary>
    /// Try to set mission fight as seen if it is behind half of travel -> flyingTo state.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> SetMissionFightAsSeen(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            await UpdateMissionStatus(current);
            
            if(current.State.CurrentState == MissionState.State.FlyingBack
                || current.State.CurrentState == MissionState.State.Finished)
            {
                if (!current.State.SeenFight)
                {
                    current.State.SeenFight = true;
                    await m_missionRepository.UpdateAsync(current);
                    return true;
                }
                    
            }
                
            return false;
        });

    /// <summary>
    /// Updates mission status from based on current time left.
    /// Expected behaviour is to start with FlyingTo state
    /// </summary>
    /// <param name="mission"></param>
    /// <returns></returns>
    private async Task UpdateMissionStatus(Mission_Database mission)
    {
        if(mission.State.CurrentState == MissionState.State.NotActive)
            return;

        var now = DateTime.UtcNow;
        var seconds = (float)(now - mission.State.LastUpdate).TotalSeconds;

        mission.State.TimeLeft = Math.Max(0, mission.State.TimeLeft - seconds);
        mission.State.LastUpdate = now;

        if (mission.State.Aborted)
        {
            if(mission.State.TimeLeft <= 0)
                mission.State.CurrentState = MissionState.State.Finished;
        }
        else
        {
            if(mission.State.TimeLeft < mission.GeneralData.Time/2.0f && mission.State.TimeLeft > 0)
                mission.State.CurrentState = MissionState.State.FlyingBack;
            else if(mission.State.TimeLeft <= 0)
                mission.State.CurrentState = MissionState.State.Finished;
        }

        await m_missionRepository.UpdateAsync(mission);
    }
}