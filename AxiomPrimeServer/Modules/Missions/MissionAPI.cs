using AxiomPrime_Metadata.Missions;

public class MissionAPI
{
    private readonly IMissionService m_missionService;
    private readonly EventBus m_eventBus;

    public MissionAPI(
        IMissionService missionService,
        EventBus eventBus)
    {
        m_missionService = missionService;
        m_eventBus = eventBus;
    }

    // =========================================================
    #region READ
    // =========================================================

    public Task<List<Mission_Database>> GetAsync(string playerId)
        => m_missionService.GetAsync(playerId);

    #endregion

    // =========================================================
    #region MISSION OPERATIONS
    // =========================================================

    public Task<bool> AddMission(string playerId, Mission_Database mission)
        => m_missionService.AddMission(playerId, mission);

    public Task<bool> UpdateMissionData(string playerId, Guid missionID, MissionGeneralData generalData)
        => m_missionService.UpdateMissionData(playerId, missionID, generalData);

    public Task<bool> UpdateMissionReward(string playerId, Guid missionID, MissionReward_Database reward)
        => m_missionService.UpdateMissionReward(playerId, missionID, reward);

    public Task<bool> RemoveMission(string playerId, Guid missionID)
        => m_missionService.RemoveMission(playerId, missionID);

    public Task<bool> StartTravelToFight(string playerId, Guid missionID, Guid shipID, int travelTime)
        => m_missionService.StartTravelToFight(playerId, missionID, shipID, travelTime);

    public Task<bool> StartTravelBack(string playerId, Guid missionID, int travelTime)
        => m_missionService.StartTravelBack(playerId, missionID, travelTime);

    public Task<bool> SkipTraveling(string playerId, Guid missionID)
        => m_missionService.SkipTraveling(playerId, missionID);

    #endregion

    // =========================================================
    #region UTILITY
    // =========================================================

    public Task<bool> IsMissionBattleReady(string playerId, Guid missionID)
        => m_missionService.IsMissionBattleReady(playerId, missionID);

    public Task<bool> IsMissionFinished(string playerId, Guid missionID)
        => m_missionService.IsMissionFinished(playerId, missionID);

    #endregion
}