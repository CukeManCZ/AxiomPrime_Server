using AxiomPrime.Generators.Missions;
using AxiomPrime.Models.Fight;
using AxiomPrime_Metadata.Missions;

public interface IMissionRegenerationService
{
    Task RegenerateMissionsAsync(string profileId, ShipStats stats);
}

public class MissionRegenerationService : IMissionRegenerationService
{
    private readonly MissionAPI m_missionAPI;
    private readonly GlobalPlayerDataAPI m_globalAPI;
    private readonly MissionGenerator m_missionGenerator = new();

    // Prevents multiple regeneration operations from running
    // simultaneously inside this server process.
    private static readonly SemaphoreSlim _regenerationLock = new(1, 1);

    public MissionRegenerationService(MissionAPI missionAPI, GlobalPlayerDataAPI globalPlayerDataAPI)
    {
        m_missionAPI = missionAPI;
        m_globalAPI = globalPlayerDataAPI;
    }

    public async Task RegenerateMissionsAsync(string profileId, ShipStats stats)
    {
        await _regenerationLock.WaitAsync();

        try
        {
            var current = await m_missionAPI.GetAsync(profileId);
            var globalData = await m_globalAPI.GetAsync(profileId);
            // Update inactive missions
            foreach (var mission in current)
            {
                if (mission.State.CurrentState != MissionState.State.NotActive)
                    continue;

                var updatedMission = m_missionGenerator.UpdateMission(
                    Mission_Database.FromDatabaseMission(mission),
                    stats);

                await m_missionAPI.UpdateMissionData(
                    profileId,
                    updatedMission.Identity.Id,
                    updatedMission.GeneralData);
            }

            // IMPORTANT:
            // Get the missions again after updates.
            current = await m_missionAPI.GetAsync(profileId);

            // Only add missions when there are fewer than 3.
            while (current.Count < 3)
            {
                await m_missionAPI.AddMission(
                    profileId,
                    Mission_Database.ToDatabaseMission(
                        profileId,
                        m_missionGenerator.GenerateMission(globalData.Level, stats)));

                // Refresh count after every insert.
                current = await m_missionAPI.GetAsync(profileId);
            }
        }
        finally
        {
            _regenerationLock.Release();
        }
    }
}
