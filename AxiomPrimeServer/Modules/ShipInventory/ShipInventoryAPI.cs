using System;
using System.Threading.Tasks;

public class ShipInventoryAPI
{
    private readonly IShipInventoryService _shipService;
    private readonly EventBus _eventBus;

    public ShipInventoryAPI(
        IShipInventoryService shipService,
        EventBus eventBus)
    {
        _shipService = shipService;
        _eventBus = eventBus;
    }

    // =========================================================
    #region READ
    // =========================================================

    public Task<ShipInventory> GetAsync(string playerId)
        => _shipService.GetAsync(playerId);

    public Task<Ship_Database> GetShipAsync(Guid shipId)
        => _shipService.GetShipAsync(shipId);

    public Task<ShipItem?> GetItemAt(Ship_Database ship, int x, int y)
        => Task.FromResult(_shipService.GetItemAt(ship, x, y));

    #endregion

    // =========================================================
    #region SHIP STATE
    // =========================================================

    public Task<bool> SelectActiveShip(string playerId, Guid shipId)
        => _shipService.SelectActiveShipAsync(playerId, shipId);

    public Task UnlockShipSlots(Guid shipId)
        => _shipService.UnlockShipSlotsAsync(shipId);

    public Task LockShipSlots(Guid shipId)
        => _shipService.LockShipSlotsAsync(shipId);

    public Task<bool> TryUnlockSlot(Guid shipId, int x, int y)
        => _shipService.TryUnlockSlotAsync(shipId, x, y);

    public Task UnlockShipInventory(Guid shipId)
        => _shipService.UnlockShipInventoryAsync(shipId);
    
    public Task LockShipInventory(Guid shipId)
        => _shipService.LockShipInventoryAsync(shipId);
    #endregion

    // =========================================================
    #region ITEM OPERATIONS
    // =========================================================

    public async Task<bool> PlaceItem(Guid shipId, Item_Database item, int x, int y)
    {
        var result = await _shipService.TryPlaceItemAsync(shipId, item, x, y);
            
        return result;

        /*await _eventBus.Publish(new ShipItemPlacedEvent
        {
            ShipId = shipId,
            ItemId = item.Id,
            X = x,
            Y = y
        });*/
    }

    public async Task<bool> PlaceItem(Guid shipId, Item_Database item)
    {
        return await _shipService.TryPlaceItemAsync(shipId, item);
    }

    public async Task<bool> RemoveItem(Guid shipId, Guid itemId)
    {
        var result = await _shipService.RemoveItemAsync(shipId, itemId);

        if (!result)
            return false;

        /*await _eventBus.Publish(new ShipItemRemovedEvent
        {
            ShipId = shipId,
            ItemId = itemId
        });*/

        return true;
    }

    #endregion

    public Task<Ship_Database> CreateShip(string playerId, ShipGrid template)
        => _shipService.CreateShipAsync(playerId, template);

    public Task AddShipSlots(string playerId, int amount)
        => _shipService.AddShipSlotsAsync(playerId, amount);
}