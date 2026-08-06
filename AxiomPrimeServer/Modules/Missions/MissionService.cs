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

    public Task<bool> StartTravelToFight(string playerId, Guid missionID, Guid shipID, int travelTime)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            current.State.ShipID = shipID;
            current.State.CurrentState = MissionState.State.FlyingTo;
            current.State.TimeLeft = travelTime;
            await m_missionRepository.UpdateAsync(current);
            return true;
        });
    
    public Task<bool> StartTravelBack(string playerId, Guid missionID, int travelTime)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            if(current.State.CurrentState != MissionState.State.BattleReady)
                return false;

            if (current.State.SkippedTravel)
            {
                current.State.TimeLeft = 0;
                current.State.CurrentState = MissionState.State.Finished;
            }else
            {
                current.State.CurrentState = MissionState.State.FlyingBack;
                current.State.TimeLeft = travelTime;
            }
            
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    public Task<bool> SkipTraveling(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            if(current.State.CurrentState != MissionState.State.FlyingTo)
            return false;

            current.State.CurrentState = MissionState.State.BattleReady;
            current.State.TimeLeft = 0;
            current.State.SkippedTravel = true;
            
            await m_missionRepository.UpdateAsync(current);
            return true;
        });

    public Task<bool> IsMissionFinished(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            await UpdateMissionTravelTime(current);
            if(current.State.TimeLeft == 0 && current.State.CurrentState == MissionState.State.FlyingBack)
            {
                current.State.CurrentState = MissionState.State.Finished;
                await m_missionRepository.UpdateAsync(current);
                return true;
            }
                
            return false;
        });

    public Task<bool> IsMissionBattleReady(string playerId, Guid missionID)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var existing = await m_missionRepository.GetAsync(playerId);
            var current = existing.FirstOrDefault(x => x.Identity.Id == missionID);
            if (current == null)
                return false;

            await UpdateMissionTravelTime(current);
            if(current.State.TimeLeft == 0 && current.State.CurrentState == MissionState.State.FlyingTo)
            {
                current.State.CurrentState = MissionState.State.BattleReady;
                await m_missionRepository.UpdateAsync(current);
                return true;
            }
                
            return false;
        });

    private async Task UpdateMissionTravelTime(Mission_Database mission)
    {
        var now = DateTime.UtcNow;
        var seconds = (float)(now - mission.State.LastUpdate).TotalSeconds;

        mission.State.TimeLeft = Math.Max(0, mission.State.TimeLeft - seconds);
        mission.State.LastUpdate = now;

        await m_missionRepository.UpdateAsync(mission);
    }
}