using System;
using System.Threading.Tasks;

public class InventoryAPI
{
    private readonly IInventoryService _inventoryService;
    private readonly EventBus _eventBus;

    public InventoryAPI(
        IInventoryService inventoryService,
        EventBus eventBus)
    {
        _inventoryService = inventoryService;
        _eventBus = eventBus;
    }

    // =========================================================
    #region READ
    // =========================================================

    public Task<Inventory> GetAsync(string playerId)
        => _inventoryService.GetAsync(playerId);

    #endregion

    // =========================================================
    #region ITEM OPERATIONS
    // =========================================================

    public Task<bool> AddItem(string playerId, Item_Database item)
        => _inventoryService.AddItem(playerId, item);

    public Task<bool> RemoveItem(string playerId, Guid itemId)
        => _inventoryService.RemoveItem(playerId, itemId);

    #endregion

    // =========================================================
    #region EQUIPMENT
    // =========================================================

    public async Task<bool> EquipItem(string playerId, Guid itemId)
    {
        var result = await _inventoryService.EquipItem(playerId, itemId);

        if (!result)
            return false;

        var inventory = await _inventoryService.GetAsync(playerId);

        var item = inventory.Items.Find(x => x.Id == itemId);

        if (item != null)
        {
            /*await _eventBus.Publish(new ItemEquippedEvent
            {
                PlayerId = playerId,
                ItemId = itemId,
                ItemName = item.ItemName
            });*/
        }

        return true;
    }

    public async Task<bool> UnEquipItem(string playerId, Guid itemId)
    {
        var result = await _inventoryService.UnEquipItem(playerId, itemId);

        if (!result)
            return false;

        return true;
    }

    #endregion

    // =========================================================
    #region UTILITY
    // =========================================================

    public Task<bool> HasSpace(string playerId)
        => _inventoryService.HasSpace(playerId);

    #endregion
}