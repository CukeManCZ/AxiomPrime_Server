using System;
using System.Threading.Tasks;

public interface IShipInventoryService
{
    // =========================================
    // GET
    // =========================================

    Task<ShipInventory> GetAsync(string playerId);
    Task<Ship> GetShipAsync(Guid shipId);

    // =========================================
    // SHIP CREATION / LIMIT SYSTEM
    // =========================================

    Task<Ship> CreateShipAsync(string playerId, ShipGrid template);

    /// <summary>
    /// Increases how many ships a player is allowed to own.
    /// </summary>
    Task AddShipSlotsAsync(string playerId, int amount);

    // =========================================
    // LOCK / UNLOCK SYSTEM
    // =========================================

    Task UnlockShipSlotsAsync(Guid shipId);
    Task LockShipSlotsAsync(Guid shipId);
    Task UnlockShipInventoryAsync(Guid shipId);
    Task LockShipInventoryAsync(Guid shipId);

    /// <summary>
    /// Unlock a single slot (player interaction)
    /// </summary>
    Task<bool> TryUnlockSlotAsync(Guid shipId, int x, int y);

    // =========================================
    // ITEM OPERATIONS
    // =========================================

    Task<bool> TryPlaceItemAsync(Guid shipId, Item item, int x, int y);

    Task<bool> RemoveItemAsync(Guid shipId, Guid itemId);

    ShipItem? GetItemAt(Ship ship, int x, int y);
}