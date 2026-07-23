using Microsoft.EntityFrameworkCore;

public class InventoryRepository
{
    private readonly GameDbContext _db;

    public InventoryRepository(GameDbContext db)
    {
        _db = db;
    }

    public async Task<Inventory> GetAsync(string playerId)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.PlayerId == playerId);

        if (inventory != null)
            return inventory;

        inventory = new Inventory
        {
            PlayerId = playerId,
            numOfItems = 20, // default max size
            Items = new List<Item>()
        };

        _db.Inventories.Add(inventory);
        await _db.SaveChangesAsync();

        return inventory;
    }

    public async Task SaveAsync(Inventory inventory)
    {
        //_db.Inventories.Update(inventory);
        await _db.SaveChangesAsync();
    }
}