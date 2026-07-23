using Utilities.DataStructures;

public static class InventoryMapper
{
    public static ItemDto ToDto(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemDto
        {
            Id = item.Id,
            Name = item.ItemName,
            Level = item.Level,
            Equipped = item.IsEquipped,
            Size = CustomGridMapper.ToDto(item.Size?.ToCustomGrid() ?? new CustomGrid<bool>(1, 1)),
            StatsData = ToDto(item.StatsData)
        };
    }

    public static InventoryDto ToDto(Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return new InventoryDto
        {
            NumOfItems = inventory.numOfItems,
            Items = inventory.Items
                .Select(ToDto)
                .ToList()
        };
    }

    public static ItemStatsDataDto ToDto(ItemStatsData statsData)
    {
        var dto = new ItemStatsDataDto();

        if (statsData is null)
            return dto;

        dto.Data = statsData.GetStats()
            .Select(ToDto)
            .ToList();

        return dto;
    }

    public static ItemStatDto ToDto(ItemStat stat)
    {
        ArgumentNullException.ThrowIfNull(stat);

        return new ItemStatDto
        {
            Name = stat.Name,
            StatType = stat.StatType,
            Value = stat.Value,
            IsPercentage = stat.IsPercentage
        };
    }
}