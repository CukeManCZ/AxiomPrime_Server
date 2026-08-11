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
    public Task<bool> RemoveMission(string playerId, Guid missionID)
        => m_missionService.RemoveMission(playerId, missionID);

    public Task<bool> UpdateMissionData(string playerId, Guid missionID, MissionGeneralData generalData)
        => m_missionService.UpdateMissionData(playerId, missionID, generalData);
    public Task<bool> UpdateMissionReward(string playerId, Guid missionID, MissionReward_Database reward)
        => m_missionService.UpdateMissionReward(playerId, missionID, reward);

    /// <summary>
    /// Starts mission travel based on set time in GeneralData.
    /// Half of time is flyingTo state -> then flyingBack state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <param name="shipID"></param>
    /// <returns></returns>
    public Task<bool> StartTravel(string playerId, Guid missionID, Guid shipID)
        => m_missionService.StartTravel(playerId, missionID, shipID);

    /// <summary>
    /// Skips all travel and sets mission into finished state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> SkipTraveling(string playerId, Guid missionID)
        => m_missionService.SkipTraveling(playerId, missionID);

    #endregion

    // =========================================================
    #region UTILITY
    // =========================================================
    /// <summary>
    /// Try to set mission fight as seen if it is behind half of travel -> flyingTo state.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> SetMissionFightAsSeen(string playerId, Guid missionID)
        => m_missionService.SetMissionFightAsSeen(playerId, missionID);

    /// <summary>
    /// Checks if mission is in finished state and fight was seen
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> IsMissionFinished(string playerId, Guid missionID)
        => m_missionService.IsMissionFinished(playerId, missionID);


    /// <summary>
    /// Player aborst mission only if it is flying to destination
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    public Task<bool> AbortMission(string playerId, Guid missionID)
        => m_missionService.AbortMission(playerId, missionID);
    #endregion
}