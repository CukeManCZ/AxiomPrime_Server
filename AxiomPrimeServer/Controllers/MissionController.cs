using AxiomPrime.ConfigLoaders;
using AxiomPrime.Generators.Enemies;
using AxiomPrime.Generators.Fight;
using AxiomPrime.Generators.Items;
using AxiomPrime.Models.Enemies;
using AxiomPrime.Models.Fight;
using AxiomPrime.Models.Items;
using AxiomPrime.Services;
using AxiomPrime_Metadata.Fight;
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
    private readonly IMissionRegenerationService m_missionRegenerationService;

    private ItemGenerator m_itemGenerator;
    private FightSequenceGenerator m_fightGenerator;
    private EnemyGenerator m_enemyGenerator;

    public MissionController(
        MissionAPI missionAPI,
        InventoryAPI inventoryAPI,
        ShipInventoryAPI shipInventoryAPI,
        GlobalPlayerDataAPI globalPlayerDataAPI,
        IMissionRegenerationService missionRegenerationService)
    {
        m_missionAPI = missionAPI;
        m_inventoryAPI = inventoryAPI;
        m_shipInventoryAPI = shipInventoryAPI;
        m_globalPlayerDataAPI = globalPlayerDataAPI;
        m_missionRegenerationService = missionRegenerationService;
        m_itemGenerator = new ItemGenerator(new AxiomPrime.Models.Stats.StatService());
        m_fightGenerator = new FightSequenceGenerator();
        m_enemyGenerator = new EnemyGenerator(new AxiomPrime.Models.Stats.StatService());
    }

    [HttpGet]
    public async Task<IActionResult> GetMissions()
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        ShipInventory inv = await m_shipInventoryAPI.GetAsync(profileId);
        Ship_Database ship = await m_shipInventoryAPI.GetShipAsync(inv.ActiveShip);
        await RegenerateMissions(profileId, StatSummer.GetShipStats( new ShipStatProvider(ship)));

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        return Ok(MissionMapper.ToDto(current));
    }

    /// <summary>
    /// Sends ship to given mission and set ship as traveling
    /// </summary>
    /// <param name="missionID"></param>
    /// <param name="shipID"></param>
    /// <returns></returns>
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

        //Set mission with ship
        bool flyMission = await m_missionAPI.StartTravel(profileId, missionID, ship.Identity.Id);
        if (flyMission)
        {
            //Set ship as traveling
            await m_shipInventoryAPI.SendToMission(ship.Identity.Id, missionID);
            return Ok("Mission started");
        }
            
        return BadRequest("Mission could not be started");
    }

    /// <summary>
    /// Player aborst mission only if it is flying to destination
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="missionID"></param>
    /// <returns></returns>
    [HttpPost("abort")]
    public async Task<IActionResult> AbortMission(Guid missionID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");

        bool abortedMission = await m_missionAPI.AbortMission(profileId, missionID);
        if(abortedMission)
            return Ok("Mission aborted");

        return BadRequest("Mission could not be aborted");
    }

    /// <summary>
    /// Tries to skip mission flight
    /// </summary>
    /// <param name="missionID"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Client wants wants to see fight sequence
    /// </summary>
    /// <param name="missionID"></param>
    /// <returns></returns>
    [HttpGet("seefight")]
    public async Task<IActionResult> SeeMissionFight(Guid missionID)
    {
        if (!User.TryGetProfileId(out var profileId))
            return Unauthorized();

        List<Mission_Database> current = await m_missionAPI.GetAsync(profileId);
        Mission_Database mission = current.First(x => x.Identity.Id == missionID);
        if(mission == null)
            return BadRequest("Mission not found");
    
        bool missionFightSeen = await m_missionAPI.SetMissionFightAsSeen(profileId, missionID);
        if (missionFightSeen)
        {
            //Generate fight
            Enemy enemy = m_enemyGenerator.GenerateEnemy("Frigate", 15);
            var ship = await m_shipInventoryAPI.GetShipAsync(mission.State.ShipID);
            if(ship ==  null) return BadRequest("Ship Does not exists");
            ShipStats playerStats = StatSummer.GetShipStats(new ShipStatProvider(ship));

            FightSequence sequence = m_fightGenerator.GetSequence(playerStats, EnemyStat.GetShipStats(enemy.Stats));
            FightSequenceDto fightSequenceDto = new FightSequenceDto
            {
                Identity = new FightSequenceIdentity()
                {
                    ID = new Guid(),
                    ID_ParticipantA = profileId,
                    Enemy = EnemyMapper.ToDto(enemy),
                    PvP = false
                },
                GeneralData = sequence.GeneralData
            };
            //Update mission reward
            if (sequence.GeneralData.ParticipantA_Won)
            {
                //Generate final Item
                if(mission.Reward.Item == null)
                {
                    mission.Reward.Item = Item_Database.ToDatabaseItem(
                        mission.Reward.GeneralData.ItemType != null 
                            ? m_itemGenerator.GenerateItem(1, mission.Reward.GeneralData.ItemType.Value) 
                            : m_itemGenerator.GenerateItem(1));

                    await m_missionAPI.UpdateMissionReward(profileId, missionID, mission.Reward);
                }

                fightSequenceDto.Reward = MissionMapper.ToDto(mission.Reward);
            }
            else
            {
                mission.Reward.GeneralData.Credits = 0;
                mission.Reward.GeneralData.Experience = 0;
                mission.Reward.GeneralData.PremiumCurrency = 0;
                mission.Reward.GeneralData.Scraps = 0;
                mission.Reward.Item = null;
                await m_missionAPI.UpdateMissionReward(profileId, missionID, mission.Reward);
                fightSequenceDto.Reward = MissionMapper.ToDto(mission.Reward);
            }
            
            //Send fight to client
            return Ok(fightSequenceDto);
        }
            

        return BadRequest("Mission fight could not be seen");

    }

    /// <summary>
    /// Finishes given mission
    /// </summary>
    /// <param name="missionID"></param>
    /// <returns></returns>
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
            //If not aborted mission give rewards
            if (!mission.State.Aborted)
            {
                await m_globalPlayerDataAPI.AddMoney(profileId, mission.Reward.GeneralData.Credits);
                await m_globalPlayerDataAPI.AddPremium(profileId, mission.Reward.GeneralData.PremiumCurrency);
                await m_globalPlayerDataAPI.AddExp(profileId, mission.Reward.GeneralData.Experience);
                await m_globalPlayerDataAPI.AddScraps(profileId, mission.Reward.GeneralData.Scraps);

                if(mission.Reward.Item != null)
                    await m_inventoryAPI.AddItem(profileId, mission.Reward.Item);
            }

            //Ship unlock
            var ship = await m_shipInventoryAPI.GetShipAsync(mission.State.ShipID);
            ArgumentNullException.ThrowIfNull(ship);
            await m_shipInventoryAPI.ReturnFromMission(ship.Identity.Id);

            await m_missionAPI.RemoveMission(profileId, missionID);
            ShipInventory inv = await m_shipInventoryAPI.GetAsync(profileId);
            Ship_Database activeShip = await m_shipInventoryAPI.GetShipAsync(inv.ActiveShip);
            await m_missionRegenerationService.RegenerateMissionsAsync(profileId, StatSummer.GetShipStats(new ShipStatProvider(activeShip)));

            return Ok("Mission finished");
        }
        return BadRequest("Mission could not be finished");
    }


    /// <summary>
    /// Updates missions based on current ship stats
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="currentShipStats"></param>
    /// <returns></returns>
    private async Task RegenerateMissions(string profileId, ShipStats currentShipStats)
    {
        await m_missionRegenerationService.RegenerateMissionsAsync(profileId, currentShipStats);
    }
}