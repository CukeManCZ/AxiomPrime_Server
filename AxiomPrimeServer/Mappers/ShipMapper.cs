using AxiomPrime.Services;
using AxiomPrime_DTOs.Inventory;
using AxiomPrime_DTOs.ShipInventory;
using Utilities.DataStructures;

public static class ShipMapper
{
    public static ShipDto ToDto(Ship_Database ship, StatSummer statSummer)
    {
        ArgumentNullException.ThrowIfNull(ship);

        ShipStatProvider shipStatProvider = new ShipStatProvider(ship);
        var shipStats = statSummer.GetShipStats(shipStatProvider);

        return new ShipDto
        {
            Identity = ship.Identity,
            GeneralData = ship.GeneralData,
            State = ship.State,
            Grid = CustomGridMapper.ToDto(ship.Grid?.ToCustomGrid() ?? new CustomGrid<string>(1, 1)),
            Items = ship.Items
                .Select(ToDto)
                .ToList(),
            Stats = new StatsDataDto
            {
                Data = shipStats.GetStats()
                    .Select(stat => new StatDto
                    {
                        Identity = stat.Identity,
                        GeneralData = stat.GeneralData
                    })
                    .ToList()
            }
        };
    }

    public static List<ShipDto> ToDto(List<Ship_Database> ships, StatSummer statSummer){
        List<ShipDto> shipDtos = new();
        foreach(var ship in ships)
            shipDtos.Add(ToDto(ship, statSummer));
        return shipDtos;
    }

    public static ShipInventoryDto ToDto(ShipInventory inventory, StatSummer statSummer)
    {
        ArgumentNullException.ThrowIfNull(inventory);


        return new ShipInventoryDto
        {
            NumOfShips = inventory.NumOfShips,
            Ships = ToDto(inventory.Ships, statSummer),
            ActiveShip = inventory.ActiveShip
        };
    }

    public static ShipItemDto ToDto(ShipItem shipItem)
    {
        ArgumentNullException.ThrowIfNull(shipItem);

        return new ShipItemDto
        {
            item = InventoryMapper.ToDto(shipItem.Item),
            X = shipItem.X,
            Y = shipItem.Y
        };
    }
}
