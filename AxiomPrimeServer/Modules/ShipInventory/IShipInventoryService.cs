public interface IShipInventoryService
{
    // =========================================
    // GET
    // =========================================

    Task<ShipInventory> GetAsync(string playerId);
    Task<Ship_Database> GetShipAsync(Guid shipId);

    // =========================================
    // SHIP CREATION / LIMIT SYSTEM
    // =========================================

    Task<Ship_Database> CreateShipAsync(string playerId, ShipGrid template);

    /// <summary>
    /// Increases how many ships a player is allowed to own.
    /// </summary>
    Task AddShipSlotsAsync(string playerId, int amount);

    // =========================================
    // SHIP SELECTION
    // =========================================

    Task<bool> SelectActiveShipAsync(string playerId, Guid shipId);

    // =========================================
    // LOCK / UNLOCK SYSTEM
    // =========================================

    Task UnlockShipSlotsAsync(Guid shipId);
    Task LockShipSlotsAsync(Guid shipId);
    Task UnlockShipInventoryAsync(Guid shipId);
    Task LockShipInventoryAsync(Guid shipId);
    Task SendToMissionAsync(Guid shipId, Guid missionId);
    Task ReturnFromMissionAsync(Guid shipId);

    /// <summary>
    /// Unlock a single slot (player interaction)
    /// </summary>
    Task<bool> TryUnlockSlotAsync(Guid shipId, int x, int y);

    // =========================================
    // ITEM OPERATIONS
    // =========================================

    Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item, int x, int y);
    Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item);

    Task<bool> RemoveItemAsync(Guid shipId, Guid itemId);

    ShipItem? GetItemAt(Ship_Database ship, int x, int y);
}