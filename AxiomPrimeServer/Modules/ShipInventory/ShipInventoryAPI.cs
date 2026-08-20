using System;
using System.Threading.Tasks;

public class ShipInventoryAPI
{
    private readonly IShipInventoryService m_shipService;
    private readonly EventBus _eventBus;

    public ShipInventoryAPI(
        IShipInventoryService shipService,
        EventBus eventBus)
    {
        m_shipService = shipService;
        _eventBus = eventBus;
    }

    // =========================================================
    #region READ
    // =========================================================

    public Task<ShipInventory> GetAsync(string playerId)
        => m_shipService.GetAsync(playerId);

    public Task<Ship_Database> GetShipAsync(Guid shipId)
        => m_shipService.GetShipAsync(shipId);

    public Task<ShipItem?> GetItemAt(Ship_Database ship, int x, int y)
        => Task.FromResult(m_shipService.GetItemAt(ship, x, y));

    #endregion

    // =========================================================
    #region SHIP STATE
    // =========================================================

    public Task<bool> SelectActiveShip(string playerId, Guid shipId)
        => m_shipService.SelectActiveShipAsync(playerId, shipId);

    public Task UnlockShipSlots(Guid shipId)
        => m_shipService.UnlockShipSlotsAsync(shipId);

    public Task LockShipSlots(Guid shipId)
        => m_shipService.LockShipSlotsAsync(shipId);

    public Task<bool> TryUnlockSlot(Guid shipId, int x, int y)
        => m_shipService.TryUnlockSlotAsync(shipId, x, y);

    public Task UnlockShipInventory(Guid shipId)
        => m_shipService.UnlockShipInventoryAsync(shipId);
    
    public Task LockShipInventory(Guid shipId)
        => m_shipService.LockShipInventoryAsync(shipId);

    public Task SendToMission(Guid shipId, Guid missionId)
        => m_shipService.SendToMissionAsync(shipId, missionId);

    public Task ReturnFromMission(Guid shipId)
        => m_shipService.ReturnFromMissionAsync(shipId);
    #endregion

    // =========================================================
    #region ITEM OPERATIONS
    // =========================================================

    public async Task<bool> PlaceItem(Guid shipId, Item_Database item, int x, int y)
    {
        var result = await m_shipService.TryPlaceItemAsync(shipId, item, x, y);
            
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
        return await m_shipService.TryPlaceItemAsync(shipId, item);
    }

    public async Task<bool> RemoveItem(Guid shipId, Guid itemId)
    {
        var result = await m_shipService.RemoveItemAsync(shipId, itemId);

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
        => m_shipService.CreateShipAsync(playerId, template);

    public Task AddShipSlots(string playerId, int amount)
        => m_shipService.AddShipSlotsAsync(playerId, amount);

    #region Experience
    public Task AddExperience(Guid shipId, int amount)
        => m_shipService.AddExp(shipId, amount);
    #endregion

    #region Energy
    public Task AddEnergy(Guid shipId, float amount)
        => m_shipService.AddEnergy(shipId, amount);

    public Task<bool> UseEnergy(Guid shipId, float amount)
        => m_shipService.UseEnergy(shipId, amount);

    public Task UpdateEnergyRegenSpeed(Guid shipId, float energyRegenSpeed)
        => m_shipService.UpdateEnergyRegenSpeed(shipId, energyRegenSpeed);
    
    public Task UpdateEnergyMaximum(Guid shipId, float energyMaximum)
        => m_shipService.UpdateEnergyMaximum(shipId, energyMaximum);
    #endregion
}