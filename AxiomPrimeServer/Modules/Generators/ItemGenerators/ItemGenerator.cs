using System;
using System.Collections.Generic;
using System.Linq;

public class ItemGenerator : IItemGenerator
{
    private float _itemValueModifier;
    private float _itemValueRandomRange;

    private List<StatList.StatDefinition> _allStats = new();
    private List<ItemData> _itemData = new();

    private readonly Random _random = new();

    public void Initialize(
        List<ItemData> itemData,
        float itemValueModifier = 10f,
        float itemValueRandomRange = 0.1f)
    {
        _allStats = StatList.Instance.GetAllStats();
        _itemValueModifier = itemValueModifier;
        _itemValueRandomRange = itemValueRandomRange;
        _itemData = itemData;
    }

    // =========================================================
    // PUBLIC API
    // =========================================================

    public Item_Database GenerateItem(int playerLvl)
    {
        var type = RandomEnum<ItemType>();
        return GenerateInternal(playerLvl, type);
    }

    public Item_Database GenerateItem(int playerLvl, ItemType itemType)
    {
        return GenerateInternal(playerLvl, itemType);
    }

    // =========================================================
    // CORE GENERATION
    // =========================================================

    private Item_Database GenerateInternal(int playerLvl, ItemType type)
    {
        if (playerLvl <= 1) playerLvl = 1;

        float itemValue =
            playerLvl *
            _itemValueModifier *
            RandomRange(1 - _itemValueRandomRange, 1 + _itemValueRandomRange);

        string subtype = GetRandomSubtype(type);

        var item = new Item_Database
        {
            Id = Guid.NewGuid(),
            Level = playerLvl,
            ItemName = "Default",
            Size = new ItemGridData(),
            StatsData = new ItemStatsData()
        };

        var possibleNames = ItemData.GetItemNamesWithSubType(subtype, _itemData);

        if (possibleNames.Count == 0)
        {
            item.ItemName = "Default";
            item.Size = new ItemGridData
            {
                Width = 1,
                Height = 1,
                Values = new List<bool> { true }
            };
        }
        else
        {
            item.ItemName = possibleNames[RandomRangeInt(0, possibleNames.Count)];

            var data = ItemData.GetItemDataWithName(item.ItemName, _itemData);
            if (data != null)
            {
                item.Size = ItemGridData.FromCustomGrid(data.GetGrid());
            }
            else
            {
                item.Size = new ItemGridData
                {
                    Width = 1,
                    Height = 1,
                    Values = new List<bool> { true }
                };
            }
        }

        itemValue *= GetSize(item.Size);

        item.Power = itemValue;
        item.Price = Math.Max(1, (int)Math.Round(itemValue));

        var stats = CreateStats(type, subtype, itemValue, item.Size);
        item.StatsData.SetStats(stats);

        return item;
    }

    // =========================================================
    // STATS
    // =========================================================

    private List<ItemStat_Database> CreateStats(ItemType type, string subtype, float itemValue, ItemGridData size)
    {
        var result = new List<ItemStat_Database>();

        var globalStats = _allStats
            .Where(s => s.AppliesTo.Contains("Global"))
            .ToList();

        var otherStats = _allStats
            .Where(s =>
                s.AppliesTo.Contains(type.ToString()) ||
                s.AppliesTo.Contains($"{type}:{subtype}")
            )
            .ToList();

        foreach (var g in globalStats)
        {
            float val = CalcGlobalStat(g.Name, itemValue, size.Width * size.Height);

            result.Add(new ItemStat_Database
            {
                Name = g.Name,
                StatType = g.Type.ToString(),
                Value = val,
                Weight = g.Weight,
                IsPercentage = g.IsPercentage
            });
        }

        if (otherStats.Count == 0)
            return result;

        var reordered = new List<StatList.StatDefinition>();

        var prioritized = otherStats.Where(s => s.MaxValuePerItem != 0).ToList();
        otherStats.RemoveAll(s => s.MaxValuePerItem != 0);

        reordered.AddRange(prioritized);
        reordered.AddRange(otherStats.OrderBy(_ => _random.Next()));

        var probabilities = GenerateProbabilities(reordered.Count);

        float remaining = 1f;

        for (int i = 0; i < reordered.Count; i++)
        {
            var stat = reordered[i];
            float prob = probabilities[i];

            float value = (float)Math.Round(prob * itemValue / stat.Weight);

            if (stat.MaxValuePerItem != 0)
            {
                if (value > stat.MaxValuePerItem)
                {
                    remaining -= prob;
                    value = stat.MaxValuePerItem;
                }
            }

            if (value <= 0)
                continue;

            result.Add(new ItemStat_Database
            {
                Name = stat.Name,
                StatType = stat.Type.ToString(),
                Value = value,
                Weight = stat.Weight,
                IsPercentage = stat.IsPercentage
            });
        }

        return result;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private float CalcGlobalStat(string name, float itemValue, int size)
    {
        return name switch
        {
            "energy consumption" => (float)Math.Round(itemValue * 0.1f),
            "size" => size,
            _ => 0
        };
    }

    private string GetRandomSubtype(ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => RandomEnum<WeaponSubtype>().ToString(),
            ItemType.Defense => RandomEnum<DefenseSubtype>().ToString(),
            ItemType.Propulsion => RandomEnum<PropulsionSubtype>().ToString(),
            ItemType.Miscellaneous => RandomEnum<MiscSubtype>().ToString(),
            _ => ""
        };
    }

    private float GetSize(ItemGridData grid)
        => grid.Width * grid.Height;

    private List<float> GenerateProbabilities(int n)
    {
        var result = new List<float>();
        float remaining = 1f;

        for (int i = 0; i < n - 1; i++)
        {
            float val = RandomRange(0f, remaining);
            result.Add(val);
            remaining -= val;
        }

        result.Add(remaining);
        return result.OrderBy(_ => _random.Next()).ToList();
    }

    private T RandomEnum<T>()
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(_random.Next(values.Length))!;
    }

    private float RandomRange(float min, float max)
        => (float)(_random.NextDouble() * (max - min) + min);

    private int RandomRangeInt(int min, int max)
        => _random.Next(min, max);

}

//TODO: DO this with loading
public enum ItemType
    {
        Weapon,
        Defense,
        Propulsion,
        Miscellaneous
    }
    public enum WeaponSubtype
    {
        Kinetic,
        Energy,
        Hybrid
    }
    public enum DefenseSubtype
    {
        Armor,
        Shield
    }
    public enum PropulsionSubtype
    {
        Thruster,
        Microwarpdrive
    }
    public enum MiscSubtype
    {
        Generator,
        Amplifier
    }