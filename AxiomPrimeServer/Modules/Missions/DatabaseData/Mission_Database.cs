using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AxiomPrime.Models.Items;
using AxiomPrime.Models.Missions;
using AxiomPrime_Metadata.Missions;
using AxiomPrime_Metadata.Rewards;

public class Mission_Database
{
    public static Mission_Database ToDatabaseMission(string playerID, Mission mission)
    {
        ArgumentNullException.ThrowIfNull(mission);

        var databaseMission = new Mission_Database
        {
            Id = mission.Identity is not null && mission.Identity.Id != Guid.Empty
                ? mission.Identity.Id
                : Guid.NewGuid(),
            PlayerId = playerID,
            Identity = mission.Identity ?? new MissionIdentity(),
            GeneralData = mission.GeneralData ?? new MissionGeneralData(),
            State = new MissionState(),
            Reward = new MissionReward_Database
            {
                GeneralData = mission.Reward.GeneralData,
                Item = mission.Reward.Item != null ? Item_Database.ToDatabaseItem(mission.Reward.Item) : null
            }
        };

        return databaseMission;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [NotMapped]
    public MissionIdentity Identity { get; set; } = new();

    [NotMapped]
    public MissionGeneralData GeneralData { get; set; } = new();

    [NotMapped]
    public MissionState State { get; set; } = new();

    [NotMapped]
    public MissionReward_Database Reward { get; set; } = new()
    {
        GeneralData = new RewardGeneralData()
    };

    [Key]
    public Guid Id { get; set; }

    public required string PlayerId { get; set; }

    public string MissionDataJson
    {
        get => JsonSerializer.Serialize(new MissionDataSnapshot
        {
            Identity = Identity,
            GeneralData = GeneralData,
            State = State,
            Reward = Reward
        }, JsonOptions);

        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Identity = new MissionIdentity();
                GeneralData = new MissionGeneralData();
                State = new MissionState();
                Reward = new MissionReward_Database
                {
                    GeneralData = new RewardGeneralData()
                };
                return;
            }

            var snapshot = JsonSerializer.Deserialize<MissionDataSnapshot>(value, JsonOptions) ?? new MissionDataSnapshot();
            Identity = snapshot.Identity ?? new MissionIdentity();
            Id = Identity.Id != Guid.Empty ? Identity.Id : Id;
            GeneralData = snapshot.GeneralData ?? new MissionGeneralData();
            State = snapshot.State ?? new MissionState();
            Reward = snapshot.Reward ?? new MissionReward_Database
            {
                GeneralData = new RewardGeneralData()
            };
        }
    }

    private sealed class MissionDataSnapshot
    {
        public MissionIdentity? Identity { get; set; }
        public MissionGeneralData? GeneralData { get; set; }
        public MissionState? State { get; set; }
        public MissionReward_Database? Reward { get; set; }
    }
}