using AxiomPrime_DTOs.ShipInventory;
using Utilities.DataStructures;

public static class ShipMapper
{
    public static ShipDto ToDto(Ship_Database ship)
    {
        ArgumentNullException.ThrowIfNull(ship);

        return new ShipDto
        {
            Identity = ship.Identity,
            GeneralData = ship.GeneralData,
            State = ship.State,
            Grid = CustomGridMapper.ToDto(ship.Grid?.ToCustomGrid() ?? new CustomGrid<string>(1, 1)),
            Items = ship.Items
                .Select(ToDto)
                .ToList()
        };
    }

    public static ShipInventoryDto ToDto(ShipInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return new ShipInventoryDto
        {
            NumOfShips = inventory.NumOfShips,
            Ships = inventory.Ships
                .Select(ToDto)
                .ToList()
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
