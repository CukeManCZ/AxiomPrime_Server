public class InventoryService : IInventoryService
{
    private readonly InventoryRepository _inventoryRepository;
    private readonly PlayerLockProvider _playerLockProvider;

    public InventoryService(
        InventoryRepository inventoryRepository,
        PlayerLockProvider playerLockProvider)
    {
        _inventoryRepository = inventoryRepository;
        _playerLockProvider = playerLockProvider;
    }

    // =========================================================
    #region GET INVENTORY
    // =========================================================

    public Task<Inventory> GetAsync(string playerId)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            return await _inventoryRepository.GetAsync(playerId);
        });

    #endregion

    // =========================================================
    #region ADD ITEM
    // =========================================================

    public Task<bool> AddItem(string playerId, Item item)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            var inventory = await _inventoryRepository.GetAsync(playerId);

            if (inventory.Items.Count >= inventory.numOfItems)
                return false;

            // prevent duplicates by ID
            if (inventory.Items.Any(x => x.Id == item.Id))
                return true;

            inventory.Items.Add(item);

            await _inventoryRepository.SaveAsync(inventory);
            return true;
        });

    #endregion

    // =========================================================
    #region REMOVE ITEM
    // =========================================================

    public Task<bool> RemoveItem(string playerId, Guid itemId)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            var inventory = await _inventoryRepository.GetAsync(playerId);

            var item = inventory.Items.FirstOrDefault(x => x.Id == itemId);
            if (item == null)
                return false;

            inventory.Items.Remove(item);

            await _inventoryRepository.SaveAsync(inventory);
            return true;
        });

    #endregion

    // =========================================================
    #region EQUIP ITEM
    // =========================================================

    public Task<bool> EquipItem(string playerId, Guid itemId)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            var inventory = await _inventoryRepository.GetAsync(playerId);

            var item = inventory.Items.FirstOrDefault(x => x.Id == itemId);
            if (item == null)
                return false;

            item.IsEquipped = true;

            await _inventoryRepository.SaveAsync(inventory);
            return true;
        });

    public Task<bool> UnEquipItem(string playerId, Guid itemId)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            var inventory = await _inventoryRepository.GetAsync(playerId);

            var item = inventory.Items.FirstOrDefault(x => x.Id == itemId);
            if (item == null)
                return false;

            item.IsEquipped = false;

            await _inventoryRepository.SaveAsync(inventory);
            return true;
        });
    #endregion

    // =========================================================
    #region UNLOCK ITEM SLOT CHECK
    // =========================================================

    public Task<bool> HasSpace(string playerId)
        => _playerLockProvider.WithLock(playerId, async () =>
        {
            var inventory = await _inventoryRepository.GetAsync(playerId);
            return inventory.Items.Count < inventory.numOfItems;
        });

    #endregion
}