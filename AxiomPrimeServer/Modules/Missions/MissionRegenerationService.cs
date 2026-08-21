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
    private readonly ShipInventoryAPI m_shipInventoryAPI;
    private readonly MissionGenerator m_missionGenerator = new();

    // Prevents multiple regeneration operations from running
    // simultaneously inside this server process.
    private static readonly SemaphoreSlim _regenerationLock = new(1, 1);

    public MissionRegenerationService(MissionAPI missionAPI, GlobalPlayerDataAPI globalPlayerDataAPI, ShipInventoryAPI shipInventoryAPI)
    {
        m_missionAPI = missionAPI;
        m_globalAPI = globalPlayerDataAPI;
        m_shipInventoryAPI = shipInventoryAPI;
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
            var shipInv = await m_shipInventoryAPI.GetAsync(profileId);
            current = await m_missionAPI.GetAsync(profileId);
            
            foreach(var ship in shipInv.Ships)
            {
                var missionsDedyceted = current.Where(x => x.Identity.GeneratedForShipID == ship.Identity.Id);
                if(missionsDedyceted.FirstOrDefault(x => x.Identity.Difficulty == MissionDifficulty.Easy) == null)
                {
                    var mission = m_missionGenerator.GenerateGenericMission(ship.GeneralData.Level, MissionDifficulty.Easy, stats);
                    mission.Identity.GeneratedForShipID = ship.Identity.Id;
                    await m_missionAPI.AddMission(
                        profileId,
                        Mission_Database.ToDatabaseMission(profileId, mission));
                }
                if(missionsDedyceted.FirstOrDefault(x => x.Identity.Difficulty == MissionDifficulty.Medium) == null)
                {
                    var mission = m_missionGenerator.GenerateGenericMission(ship.GeneralData.Level, MissionDifficulty.Medium, stats);
                    mission.Identity.GeneratedForShipID = ship.Identity.Id;
                    await m_missionAPI.AddMission(
                        profileId,
                        Mission_Database.ToDatabaseMission(profileId, mission));
                }
                if(missionsDedyceted.FirstOrDefault(x => x.Identity.Difficulty == MissionDifficulty.Hard) == null)
                {
                    var mission = m_missionGenerator.GenerateGenericMission(ship.GeneralData.Level, MissionDifficulty.Hard, stats);
                    mission.Identity.GeneratedForShipID = ship.Identity.Id;
                    await m_missionAPI.AddMission(
                        profileId,
                        Mission_Database.ToDatabaseMission(profileId, mission));
                }
            }
            
        }
        finally
        {
            _regenerationLock.Release();
        }
    }
}
