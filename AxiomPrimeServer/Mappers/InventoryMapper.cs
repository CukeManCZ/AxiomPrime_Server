using AxiomPrime_DTOs.Inventory;
using AxiomPrime_Metadata.General;
using Utilities.DataStructures;

public static class InventoryMapper
{
    public static ItemDto ToDto(Item_Database item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ItemDto
        {
            Identity = item.Identity,
            GeneralData = item.GeneralData,
            State = item.State,
            Size = CustomGridMapper.ToDto(item.Size?.ToCustomGrid() ?? new CustomGrid<bool>(1, 1)),
            Stats = ToDto(item.StatsData)
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

    public static StatsDataDto ToDto(ItemStatsData statsData)
    {
        var dto = new StatsDataDto();

        if (statsData is null)
            return dto;

        dto.Data = statsData.GetStats()
            .Select(ToDto)
            .ToList();

        return dto;
    }

    public static StatDto ToDto(ItemStat_Database stat)
    {
        ArgumentNullException.ThrowIfNull(stat);

        return new StatDto
        {
            Identity = stat.Identity,
            GeneralData = stat.GeneralData
        };
    }
}
