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

    /// <summary>
    /// Resolves which player owns a ship. Used to pick the per player lock that
    /// guards the ship document, so callers only need the ship id.
    /// </summary>
    public async Task<string> GetOwnerIdOfShip(Guid shipId)
    {
        var ownerId = await m_db.Ships
            .Where(x => x.Id == shipId)
            .Select(x => x.ShipInventoryId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new InvalidOperationException(
                $"Ship with id '{shipId}' does not exist.");

        return ownerId;
    }

    public Task SaveAsync()
        => m_db.SaveChangesAsync();

    /// <summary>
    /// Forgets the ship rows this request has already loaded, so the next read goes
    /// back to the database and sees what other requests committed in the meantime.
    ///
    /// A locked ship operation calls this before loading the ship it is about to
    /// overwrite. Without it EF returns the instance this request read before it
    /// acquired the lock, and saving that instance would rewrite the whole ship
    /// document from stale data and silently undo the other request.
    ///
    /// Item rows are deliberately left tracked. Callers hand Item_Database objects
    /// loaded by the inventory module into PlaceItem, and an untracked item reached
    /// through a new ShipItem would be treated as a new entity and inserted again.
    /// </summary>
    public void DiscardTrackedShips()
    {
        var entries = m_db.ChangeTracker
            .Entries()
            .Where(e => e.Entity is ShipInventory
                     || e.Entity is Ship_Database
                     || e.Entity is ShipItem)
            .ToList();

        foreach (var entry in entries)
            entry.State = EntityState.Detached;
    }

    public async Task AddShipAsync(Ship_Database ship)
    {
        m_db.Ships.Add(ship);
        await m_db.SaveChangesAsync();
    }
}