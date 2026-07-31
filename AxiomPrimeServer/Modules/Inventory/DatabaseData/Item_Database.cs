using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AxiomPrime.Models.Items;
using AxiomPrime_Metadata.General;
using AxiomPrime_Metadata.Inventory;

public class Item_Database
{
    public static Item_Database ToDatabaseItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var databaseItem = new Item_Database
        {
            Identity = item.Identity,
            State = item.State,
            GeneralData = item.GeneralData,
            ItemName = item.Identity?.Name ?? "Testing",
            Power = item.Power
        };

        if (item.Stats is { Count: > 0 })
        {
            var databaseStats = item.Stats
                .Where(stat => stat?.Identity is not null && stat.GeneralData is not null)
                .Select(stat => new ItemStat_Database
                {
                    Identity = stat.Identity,
                    GeneralData = stat.GeneralData
                })
                .ToList();

            databaseItem.StatsData.SetStats(databaseStats);
        }

        var sizeValue = 1;
        //TODO: work this in generator
        databaseItem.Size = CreateItemGridData(sizeValue);

        return databaseItem;
    }

    private static ItemGridData CreateItemGridData(float sizeValue)
    {
        var effectiveSize = Math.Max(1, (int)Math.Ceiling(sizeValue));
        var width = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(effectiveSize)));
        var height = Math.Max(1, (int)Math.Ceiling(effectiveSize / (double)width));

        var grid = new ItemGridData
        {
            Width = width,
            Height = height
        };

        grid.Values = new List<bool>(width * height);
        for (var i = 0; i < width * height; i++)
        {
            grid.Values.Add(i < effectiveSize);
        }

        return grid;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [NotMapped]
    public ItemIdentity Identity { get; set; } = new();

    [NotMapped]
    public ItemGeneralData GeneralData { get; set; } = new();

    [NotMapped]
    public ItemState State { get; set; } = new();

    public Guid Id
    {
        get => Identity?.Id ?? Guid.Empty;
        set
        {
            Identity ??= new ItemIdentity();
            Identity.Id = value;
        }
    }

    [NotMapped]
    public string? ItemName
    {
        get => Identity?.Name;
        set
        {
            Identity ??= new ItemIdentity();
            Identity.Name = value;
        }
    }

    [NotMapped]
    public int Level
    {
        get => GeneralData?.Level ?? 0;
        set
        {
            GeneralData ??= new ItemGeneralData();
            GeneralData.Level = value;
        }
    }

    [NotMapped]
    public int Price
    {
        get => GeneralData?.Price ?? 0;
        set
        {
            GeneralData ??= new ItemGeneralData();
            GeneralData.Price = value;
        }
    }

    [NotMapped]
    public bool IsEquipped
    {
        get => State?.Equiped ?? false;
        set
        {
            State ??= new ItemState();
            State.Equiped = value;
        }
    }

    public float Power { get; set; }

    [NotMapped]
    public ItemGridData Size { get; set; } = new();

    [NotMapped]
    public ItemStatsData StatsData { get; set; } = new();

    public string ItemDataJson
    {
        get => JsonSerializer.Serialize(new ItemDataSnapshot
        {
            Identity = Identity,
            GeneralData = GeneralData,
            State = State,
            Power = Power,
            Size = Size,
            StatsData = StatsData
        }, JsonOptions);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Identity = new ItemIdentity();
                GeneralData = new ItemGeneralData();
                State = new ItemState();
                Power = 0f;
                Size = new ItemGridData();
                StatsData = new ItemStatsData();
                return;
            }

            var snapshot = JsonSerializer.Deserialize<ItemDataSnapshot>(value, JsonOptions) ?? new ItemDataSnapshot();
            Identity = snapshot.Identity ?? new ItemIdentity();
            GeneralData = snapshot.GeneralData ?? new ItemGeneralData();
            State = snapshot.State ?? new ItemState();
            Power = snapshot.Power;
            Size = snapshot.Size ?? new ItemGridData();
            StatsData = snapshot.StatsData ?? new ItemStatsData();
        }
    }

    private sealed class ItemDataSnapshot
    {
        public ItemIdentity? Identity { get; set; }
        public ItemGeneralData? GeneralData { get; set; }
        public ItemState? State { get; set; }
        public float Power { get; set; }
        public ItemGridData? Size { get; set; }
        public ItemStatsData? StatsData { get; set; }
    }
}