using AxiomPrime.Generators.Items;
using AxiomPrime.Generators.Missions;
using Microsoft.AspNetCore.Mvc;
using Utilities.AuthorizationTools;

[ApiController]
[Route("missions")]
public class MissionController : ControllerBase
{
    private readonly InventoryAPI m_inventoryAPI;
    private readonly ShipInventoryAPI m_shipInventoryAPI;
    private readonly MissionAPI m_missionAPI;
    private readonly GlobalPlayerDataAPI m_globalPlayerDataAPI;

    private MissionGenerator m_missionGenerator;
    private ItemGenerator m_itemGenerator;

    public MissionController(MissionAPI missionAPI, InventoryAPI inventoryAPI, ShipInventoryAPI shipInventoryAPI, GlobalPlayerDataAPI globalPlayerDataAPI)
    {
        m_missionAPI = missionAPI;
        m_inventoryAPI = inventoryAPI;
        m_shipInventoryAPI = shipInventoryAPI;
        m_globalPlayerDataAPI = globalPlayerDataAPI;
        m_missionGenerator = new MissionGenerator();
        m_itemGenerator = new ItemGenerator(new AxiomPrime.Models.Stats.StatService());
    }

    [HttpGet]
    public async Task<IActionResult> GetMissions()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        await RegenerateMissions(profileId);

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        return Ok(MissionMapper.ToDto(current));
    }

    [HttpPost("flyto")]
    public async Task<IActionResult> FlyMission(Guid missionID, Guid shipID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");

        var ship = await m_shipInventoryAPI.GetShipAsync(shipID);
        if(ship == null)
            return BadRequest("Ship not found");
        if(ship.IsLocked)
            return BadRequest("Ship can not travel multiple missions");

        //Check ship
        bool flyMission = await m_missionAPI.StartTravelToFight(profileId, missionID, ship.Identity.Id, 10);
        if (flyMission)
        {
            await m_shipInventoryAPI.LockShipInventory(ship.Identity.Id);
            return Ok("Mission started");
        }
            

        return BadRequest("Mission could not be started");
    }

    [HttpPost("skip")]
    public async Task<IActionResult> SkipMission(Guid missionID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");
        
        bool skippedMission = await m_missionAPI.SkipTraveling(profileId, missionID);
        if(skippedMission)
            return Ok("Mission skipped");
        
        return BadRequest("Mission could not be skipped");
    }

    [HttpPost("flyfrom")]
    public async Task<IActionResult> FlyFromMission(Guid missionID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");

        bool startedToFlyBack = await m_missionAPI.StartTravelBack(profileId, missionID, 10);
        if(startedToFlyBack)
            return Ok("Travel from mission started");

        return BadRequest("Travel from mission could not be started");
    }

    [HttpPost("finish")]
    public async Task<IActionResult> FinishMission(Guid missionID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");

        bool missionFinished = await m_missionAPI.IsMissionFinished(profileId, missionID);

        if (missionFinished)
        {
           //TODO: Give rewards
            await m_globalPlayerDataAPI.AddMoney(profileId, mission.Reward.GeneralData.Credits);
            await m_globalPlayerDataAPI.AddPremium(profileId, mission.Reward.GeneralData.PremiumCurrency);
            await m_globalPlayerDataAPI.AddExp(profileId, mission.Reward.GeneralData.Experience);
            await m_globalPlayerDataAPI.AddScraps(profileId, mission.Reward.GeneralData.Scraps);

            if(mission.Reward.Item != null)
                await m_inventoryAPI.AddItem(profileId, mission.Reward.Item);
            else
                await m_inventoryAPI.AddItem(profileId, Item_Database.ToDatabaseItem(
                    mission.Reward.GeneralData.ItemType != null 
                        ? m_itemGenerator.GenerateItem(1, mission.Reward.GeneralData.ItemType.Value) 
                        : m_itemGenerator.GenerateItem(1)));


            //Ship unlock
            var ship = await m_shipInventoryAPI.GetShipAsync(mission.State.ShipID);
            ArgumentNullException.ThrowIfNull(ship);
            await m_shipInventoryAPI.UnlockShipInventory(ship.Identity.Id);

            await m_missionAPI.RemoveMission(profileId, missionID);
            await RegenerateMissions(profileId);


            return Ok("Mission finished");
        }
        return BadRequest("Mission could not be finished");
    }

    private async Task RegenerateMissions(string profileId)
    {
        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        for(int i = current.Count; i < 3; ++i)
            await m_missionAPI.AddMission(profileId,
                Mission_Database.ToDatabaseMission(profileId, m_missionGenerator.GenerateMission(1))
        );
    }
}