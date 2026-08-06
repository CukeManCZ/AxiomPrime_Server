using AxiomPrime.Generators.Missions;
using AxiomPrime.Models.Fight;

public interface IMissionRegenerationService
{
    Task RegenerateMissionsAsync(string profileId, ShipStats stats);
}

public class MissionRegenerationService : IMissionRegenerationService
{
    private readonly MissionAPI m_missionAPI;
    private readonly MissionGenerator m_missionGenerator = new();

    public MissionRegenerationService(MissionAPI missionAPI)
    {
        m_missionAPI = missionAPI;
    }

    public async Task RegenerateMissionsAsync(string profileId, ShipStats stats)
    {
        var current = await m_missionAPI.GetAsync(profileId);

        foreach (var mission in current)
        {
            var updatedMission = m_missionGenerator.UpdateMission(
                Mission_Database.FromDatabaseMission(mission),
                stats);

            await m_missionAPI.UpdateMissionData(
                profileId,
                updatedMission.Identity.Id,
                updatedMission.GeneralData);
        }

        for (var i = current.Count; i < 3; ++i)
        {
            await m_missionAPI.AddMission(
                profileId,
                Mission_Database.ToDatabaseMission(
                    profileId,
                    m_missionGenerator.GenerateMission(1, stats)));
        }
    }
}
