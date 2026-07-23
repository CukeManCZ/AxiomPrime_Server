using System;
using System.Threading.Tasks;

public interface IInventoryService
{
    /// <summary>
    /// Returns full inventory (with items + stats loaded)
    /// </summary>
    Task<Inventory> GetAsync(string playerId);

    /// <summary>
    /// Adds item to inventory if space allows
    /// </summary>
    Task<bool> AddItem(string playerId, Item item);

    /// <summary>
    /// Removes item by Id
    /// </summary>
    Task<bool> RemoveItem(string playerId, Guid itemId);

    /// <summary>
    /// Marks item as equipped
    /// </summary>
    Task<bool> EquipItem(string playerId, Guid itemId);

    /// <summary>
    /// Marks item as not equipped
    /// </summary>
    Task<bool> UnEquipItem(string playerId, Guid itemId);

    /// <summary>
    /// Optional helper: checks if inventory has free slot
    /// </summary>
    Task<bool> HasSpace(string playerId);
}