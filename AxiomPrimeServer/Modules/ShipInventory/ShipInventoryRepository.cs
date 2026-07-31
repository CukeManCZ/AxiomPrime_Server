using Microsoft.EntityFrameworkCore;

public class ShipInventoryRepository
{
    private readonly GameDbContext m_db;

    public ShipInventoryRepository(GameDbContext db)
    {
        m_db = db;
    }

    public async Task<ShipInventory> GetAsync(string playerId)
    {
        var inventory = await m_db.ShipInventories
            .Include(x => x.Ships)
            .ThenInclude(s => s.Items)
            .ThenInclude(si => si.Item)
            .FirstOrDefaultAsync(x => x.PlayerId == playerId);

        if (inventory != null)
            return inventory;

        inventory = new ShipInventory
        {
            PlayerId = playerId,
            NumOfShips = 100
        };

        m_db.ShipInventories.Add(inventory);
        await m_db.SaveChangesAsync();

        return inventory;
    }

    public Task<Ship_Database> GetShipAsync(Guid shipId)
        => m_db.Ships
            .Include(x => x.Items)
            .ThenInclude(si => si.Item)
            .FirstAsync(x => x.Id == shipId);

    public Task SaveAsync()
        => m_db.SaveChangesAsync();

    public async Task AddShipAsync(Ship_Database ship)
    {
        m_db.Ships.Add(ship);
        await m_db.SaveChangesAsync();
    }
}