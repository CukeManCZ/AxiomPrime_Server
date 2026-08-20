using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AxiomPrime_Metadata.Ship;

public class Ship_Database
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [NotMapped]
    public ShipIdentity Identity { get; set; } = new();

    [NotMapped]
    public ShipState State { get; set; } = new();

    [NotMapped]
    public ShipGeneralData GeneralData { get; set; } = new();

    public string ShipInventoryId { get; set; } = default!;
    public ShipInventory ShipInventory { get; set; } = default!;

    [NotMapped]
    public ShipGrid Grid { get; set; } = default!;

    [NotMapped]
    public List<ShipItem> Items { get; set; } = new();

    public Guid Id
    {
        get => Identity?.Id ?? Guid.Empty;
        set
        {
            Identity ??= new ShipIdentity();
            Identity.Id = value;
        }
    }

    public string? Name
    {
        get => Identity?.Name;
        set
        {
            Identity ??= new ShipIdentity();
            Identity.Name = value;
        }
    }

    public string? Type
    {
        get => Identity?.Type;
        set
        {
            Identity ??= new ShipIdentity();
            Identity.Type = value;
        }
    }

    public int XOrigin
    {
        get => Identity?.XOrigin ?? 0;
        set
        {
            Identity ??= new ShipIdentity();
            Identity.XOrigin = value;
        }
    }

    public int YOrigin
    {
        get => Identity?.YOrigin ?? 0;
        set
        {
            Identity ??= new ShipIdentity();
            Identity.YOrigin = value;
        }
    }

    public bool Locked
    {
        get => State?.Locked ?? false;
        set
        {
            State ??= new ShipState();
            State.Locked = value;
        }
    }

    [NotMapped]
    public bool IsLocked
    {
        get => Locked;
        set => Locked = value;
    }

    public int Level
    {
        get => GeneralData?.Level ?? 0;
        set
        {
            GeneralData ??= new ShipGeneralData();
            GeneralData.Level = value;
        }
    }

    public int CurrentExp
    {
        get => GeneralData?.CurrentExperience ?? 0;
        set
        {
            GeneralData ??= new ShipGeneralData();
            GeneralData.CurrentExperience = value;
        }
    }

    public int MaxExp
    {
        get => GeneralData?.NextLevelExperience ?? 0;
        set
        {
            GeneralData ??= new ShipGeneralData();
            GeneralData.NextLevelExperience = value;
        }
    }

    public string ShipDataJson
    {
        get => JsonSerializer.Serialize(new ShipDataSnapshot
        {
            Identity = Identity,
            State = State,
            GeneralData = GeneralData,
            Grid = Grid
        }, JsonOptions);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Identity = new ShipIdentity();
                State = new ShipState();
                GeneralData = new ShipGeneralData();
                Grid = new ShipGrid();
                return;
            }

            var snapshot = JsonSerializer.Deserialize<ShipDataSnapshot>(value, JsonOptions) ?? new ShipDataSnapshot();
            Identity = snapshot.Identity ?? new ShipIdentity();
            State = snapshot.State ?? new ShipState();
            GeneralData = snapshot.GeneralData ?? new ShipGeneralData();
            Grid = snapshot.Grid ?? new ShipGrid();
        }
    }

    private sealed class ShipDataSnapshot
    {
        public ShipIdentity? Identity { get; set; }
        public ShipState? State { get; set; }
        public ShipGeneralData? GeneralData { get; set; }
        public ShipGrid? Grid { get; set; }
    }
}