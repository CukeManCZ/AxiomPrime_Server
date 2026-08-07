public class ShipInventoryService : IShipInventoryService
{
    private readonly ShipInventoryRepository m_repo;

    public ShipInventoryService(ShipInventoryRepository repo)
    {
        m_repo = repo;
    }

    // =========================================
    // GET
    // =========================================

    public Task<ShipInventory> GetAsync(string playerId)
        => m_repo.GetAsync(playerId);

    public Task<Ship_Database> GetShipAsync(Guid shipId)
        => m_repo.GetShipAsync(shipId);

    // =========================================
    // CREATE SHIP (NOW WITH LIMIT CHECK)
    // =========================================

    public async Task<Ship_Database> CreateShipAsync(string playerId, ShipGrid template)
    {
        var inventory = await m_repo.GetAsync(playerId);

        // CURRENT SHIP COUNT
        int currentShips = inventory.Ships.Count;
        int maxShips = inventory.NumOfShips;

        if (currentShips >= maxShips)
            throw new InvalidOperationException(
                $"Ship limit reached ({currentShips}/{maxShips})"
            );

        var ship = new Ship_Database
        {
            Id = Guid.NewGuid(),
            ShipInventoryId = playerId,
            Grid = template,
            IsLocked = false,
            Items = new List<ShipItem>()
        };

        inventory.Ships.Add(ship);

        await m_repo.AddShipAsync(ship);
        await m_repo.SaveAsync();

        return ship;
    }

    // =========================================
    // INCREASE SHIP CAPACITY
    // =========================================

    public async Task AddShipSlotsAsync(string playerId, int amount)
    {
        if (amount <= 0)
            return;

        var inventory = await m_repo.GetAsync(playerId);

        inventory.NumOfShips += amount;

        await m_repo.SaveAsync();
    }

    public async Task<bool> SelectActiveShipAsync(string playerId, Guid shipId)
    {
        var inventory = await m_repo.GetAsync(playerId);

        if (shipId == Guid.Empty)
            return false;

        var ship = inventory.Ships.FirstOrDefault(x => x.Id == shipId);
        if (ship == null)
            return false;

        inventory.ActiveShip = shipId;
        await m_repo.SaveAsync();
        return true;
    }

    // =========================================
    // CONSTANTS (STRING STATES ONLY HERE)
    // =========================================

    private const string EMPTY = "Empty";
    private const string VOID = "Void";
    private const string LOCKED = "Locked";
    private const string UNLOCKABLE = "Unlockable";

    // =========================================
    // LOCK / UNLOCK SYSTEM | Used for slots unlocking
    // =========================================

    #region Slot managing
    public async Task UnlockShipSlotsAsync(Guid shipId)
    {
        var ship = await GetShipAsync(shipId);
        var grid = ship.Grid;

        int w = grid.Width;
        int h = grid.Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var cell = grid.Get(x, y);

                if (cell == LOCKED ||
                    cell == UNLOCKABLE ||
                    cell == VOID)
                    continue;

                TryUnlockNeighbors(grid, x, y);
            }
        }

        await m_repo.SaveAsync();
    }

    public async Task LockShipSlotsAsync(Guid shipId)
    {
        var ship = await GetShipAsync(shipId);
        var grid = ship.Grid;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (grid.Get(x, y) == UNLOCKABLE)
                {
                    grid.Set(x, y, UNLOCKABLE);
                }
            }
        }

        await m_repo.SaveAsync();
    }

    private void TryUnlockNeighbors(ShipGrid grid, int x, int y)
    {
        TryUnlockCell(grid, x - 1, y);
        TryUnlockCell(grid, x + 1, y);
        TryUnlockCell(grid, x, y - 1);
        TryUnlockCell(grid, x, y + 1);
    }

    private void TryUnlockCell(ShipGrid grid, int x, int y)
    {
        if (x < 0 || y < 0 || x >= grid.Width || y >= grid.Height)
            return;

        if (grid.Get(x, y) == LOCKED)
        {
            grid.Set(x, y, UNLOCKABLE);
        }
    }

    public async Task<bool> TryUnlockSlotAsync(Guid shipId, int x, int y)
    {
        var ship = await GetShipAsync(shipId);

        var grid = ship.Grid;

        if (grid.Get(x, y) != UNLOCKABLE)
            return false;

        grid.Set(x, y, EMPTY);

        await m_repo.SaveAsync();
        return true;
    }
    #endregion

    #region LockShip
    public async Task UnlockShipInventoryAsync(Guid shipId)
    {
        var ship = await GetShipAsync(shipId);
        ship.IsLocked = false;
        await m_repo.SaveAsync();
    }
    public async Task LockShipInventoryAsync(Guid shipId)
    {
        var ship = await GetShipAsync(shipId);
        ship.IsLocked = true;
        await m_repo.SaveAsync();
    }
    #endregion

    #region Missions
    /// <summary>
    /// Sends ship on mission and locks inventory
    /// </summary>
    /// <param name="shipId"></param>
    /// <param name="missionId"></param>
    /// <returns></returns>
    public async Task SendToMissionAsync(Guid shipId, Guid missionId)
    {
        var ship = await GetShipAsync(shipId);
        ship.State.MissionID = missionId;
        ship.State.Traveling = true;
        await LockShipInventoryAsync(shipId);
    }

    /// <summary>
    /// Returns ship from mission and unlocks inventory
    /// </summary>
    /// <param name="shipId"></param>
    /// <returns></returns>
    public async Task ReturnFromMissionAsync(Guid shipId)
    {
        var ship = await GetShipAsync(shipId);
        ship.State.Traveling = false;
        await UnlockShipInventoryAsync(shipId);

    }
    #endregion
    #region Item managing
    /// <summary>
    /// PLACE ITEM
    /// </summary>
    /// <param name="shipId"></param>
    /// <param name="item"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public async Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item, int x, int y)
    {
        var ship = await GetShipAsync(shipId);

        if (item.IsEquipped)
            return false;
        if (ship.IsLocked)
            return false;

        if (!CanPlaceItem(ship, item, x, y))
            return false;

        var grid = item.Size;

        int w = grid.Width;
        int h = grid.Height;

        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                int index = iy * w + ix;

                if (!grid.Values[index])
                    continue;

                ship.Grid.Set(x + ix, y + iy, item.Id.ToString());
            }
        }

        ship.Items.Add(new ShipItem
        {
            Id = item.Id,
            ShipId = shipId,
            ItemId = item.Id,
            Item = item,
            X = x,
            Y = y
        });

        await m_repo.SaveAsync();
        return true;
    }

    public async Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item)
    {
        var ship = await GetShipAsync(shipId);
        if(item.IsEquipped)
            return false;
        if (ship.IsLocked)
            return false;

        for (int x = 0; x < ship.Grid.Width; x++)
        {
            for (int y = 0; y < ship.Grid.Height; y++)
            {
                if(CanPlaceItem(ship, item, x, y))
                    if(await TryPlaceItemAsync(shipId, item, x, y))
                        return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// REMOVE ITEM (FULL GRID SCAN LIKE UNITY)
    /// </summary>
    /// <param name="shipId"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<bool> RemoveItemAsync(Guid shipId, Guid itemId)
    {
        var ship = await GetShipAsync(shipId);
        if (ship.IsLocked)
            return false;
        var shipItem = ship.Items.FirstOrDefault(x => x.Id == itemId);
        if (shipItem == null)
            return false;

        for (int x = 0; x < ship.Grid.Width; x++)
        {
            for (int y = 0; y < ship.Grid.Height; y++)
            {
                if (ship.Grid.Get(x, y) == itemId.ToString())
                {
                    ship.Grid.Set(x, y, EMPTY);
                }
            }
        }

        ship.Items.Remove(shipItem);

        await m_repo.SaveAsync();
        return true;
    }

    /// <summary>
    /// GET ITEM AT POSITION
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public ShipItem? GetItemAt(Ship_Database ship, int x, int y)
    {
        var cell = ship.Grid.Get(x, y);

        if (cell == EMPTY ||
            cell == VOID ||
            cell == LOCKED ||
            cell == UNLOCKABLE)
            return null;

        if (!Guid.TryParse(cell, out var id))
            return null;

        return ship.Items.FirstOrDefault(i => i.Id == id);
    }

    

    /// <summary>
    /// VALIDATION
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="item"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool CanPlaceItem(Ship_Database ship, Item_Database item, int x, int y)
    {
        var grid = item.Size;

        int w = grid.Width;
        int h = grid.Height;

        if (x < 0 || y < 0 ||
            x + w > ship.Grid.Width ||
            y + h > ship.Grid.Height)
            return false;

        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                int index = iy * w + ix;

                if (!grid.Values[index])
                    continue;

                if (ship.Grid.Get(x + ix, y + iy) != EMPTY)
                    return false;
            }
        }

        return true;
    }

    #endregion
}