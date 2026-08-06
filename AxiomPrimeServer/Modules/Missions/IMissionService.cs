using AxiomPrime_Metadata.Missions;

public interface IMissionService
{
    Task<List<Mission_Database>> GetAsync(string playerId);
    Task<bool> AddMission(string playerId, Mission_Database mission);
    Task<bool> UpdateMissionData(string playerId, Guid missionID, MissionGeneralData generalData);
    Task<bool> UpdateMissionReward(string playerId,Guid missionID, MissionReward_Database reward);
    Task<bool> RemoveMission(string playerId, Guid missionID);
    Task<bool> StartTravelToFight(string playerId, Guid missionID, Guid shipID, int travelTime);
    Task<bool> StartTravelBack(string playerId, Guid missionID, int travelTime);
    Task<bool> SkipTraveling(string playerId, Guid missionID);
    Task<bool> IsMissionBattleReady(string playerId, Guid missionID);
    Task<bool> IsMissionFinished(string playerId, Guid missionID);
}