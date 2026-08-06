using System;
using System.Threading.Tasks;

public class InventoryAPI
{
    private readonly IInventoryService m_inventoryService;
    private readonly EventBus m_eventBus;

    public InventoryAPI(
        IInventoryService inventoryService,
        EventBus eventBus)
    {
        m_inventoryService = inventoryService;
        m_eventBus = eventBus;
    }

    // =========================================================
    #region READ
    // =========================================================

    public Task<Inventory> GetAsync(string playerId)
        => m_inventoryService.GetAsync(playerId);

    #endregion

    // =========================================================
    #region ITEM OPERATIONS
    // =========================================================

    public Task<bool> AddItem(string playerId, Item_Database item)
        => m_inventoryService.AddItem(playerId, item);

    public Task<bool> RemoveItem(string playerId, Guid itemId)
        => m_inventoryService.RemoveItem(playerId, itemId);

    #endregion

    // =========================================================
    #region EQUIPMENT
    // =========================================================

    public async Task<bool> EquipItem(string playerId, Guid itemId)
    {
        var result = await m_inventoryService.EquipItem(playerId, itemId);

        if (!result)
            return false;

        var inventory = await m_inventoryService.GetAsync(playerId);

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
        var result = await m_inventoryService.UnEquipItem(playerId, itemId);

        if (!result)
            return false;

        return true;
    }

    #endregion

    // =========================================================
    #region UTILITY
    // =========================================================

    public Task<bool> HasSpace(string playerId)
        => m_inventoryService.HasSpace(playerId);

    #endregion
}