using AxiomPrime_Metadata.Missions;

public interface IMissionService
{
    Task<List<Mission_Database>> GetAsync(string playerId);
    Task<bool> AddMission(string playerId, Mission_Database mission);
    Task<bool> RemoveMission(string playerId, Guid missionID);

    Task<bool> UpdateMissionData(string playerId, Guid missionID, MissionGeneralData generalData);
    Task<bool> UpdateMissionReward(string playerId,Guid missionID, MissionReward_Database reward);
    
    /// <summary>
    /// Starts mission travel based on set time in GeneralData.
    /// Half of time is flyingTo state -> then flyingBack state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <param name="shipID"></param>
    /// <returns></returns>
    Task<bool> StartTravel(string playerId, Guid missionID, Guid shipID);
    /// <summary>
    /// Skips all travel and sets mission into finished state
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    Task<bool> SkipTraveling(string playerId, Guid missionID);
    /// <summary>
    /// Try to set mission fight as seen if it is behind half of travel -> flyingTo state.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    Task<bool> SetMissionFightAsSeen(string playerId, Guid missionID);
    /// <summary>
    /// Checks if mission is in finished state and fight was seen
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    Task<bool> IsMissionFinished(string playerId, Guid missionID);

    /// <summary>
    /// Player aborted mission, he will travel back according how long he traveled;
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    Task<bool> AbortMission(string playerId, Guid missionID);
}