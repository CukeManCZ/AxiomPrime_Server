using AxiomPrime.Models.Ship;
using AxiomPrime.Services;
using AxiomPrime_Metadata.Ship;

public class ShipInventoryService : IShipInventoryService
{
    private readonly ShipInventoryRepository m_repo;
    private readonly PlayerLockProvider m_playerLockProvider;

    public ShipInventoryService(
        ShipInventoryRepository repo,
        PlayerLockProvider playerLockProvider)
    {
        m_repo = repo;
        m_playerLockProvider = playerLockProvider;
    }

    // =========================================================
    // CONCURRENCY
    //
    // Everything about a ship (level, experience, energy, grid,
    // slots, lock state) lives in one jsonb document per ship, so
    // every save overwrites the whole document. Two overlapping
    // operations would silently discard one of them, so every
    // operation that saves runs inside the per player lock.
    //
    // Naming rule:
    //   Public  ...Async   - acquires the lock, calls the core
    //   Private ...Locked  - assumes the lock is ALREADY held and
    //                        only ever calls other ...Locked
    //                        methods. Never acquires the lock,
    //                        because the semaphore is not
    //                        reentrant and would deadlock.
    //
    // Read only operations are not locked, they never write.
    // =========================================================

    private async Task<T> WithShipLockAsync<T>(Guid shipId, Func<Task<T>> action)
    {
        var playerId = await m_repo.GetOwnerIdOfShip(shipId);
        return await m_playerLockProvider.WithLock(playerId, action);
    }

    private async Task WithShipLockAsync(Guid shipId, Func<Task> action)
    {
        var playerId = await m_repo.GetOwnerIdOfShip(shipId);
        await m_playerLockProvider.WithLock(playerId, action);
    }

    private Task<T> WithPlayerLockAsync<T>(string playerId, Func<Task<T>> action)
        => m_playerLockProvider.WithLock(playerId, action);

    private Task WithPlayerLockAsync(string playerId, Func<Task> action)
        => m_playerLockProvider.WithLock(playerId, action);

    // =========================================
    // GET
    // =========================================

    #region Get Data
    public async Task<ShipInventory> GetAsync(string playerId)
    {
        await UpdateEnergyGeneration(playerId);
        return await m_repo.GetAsync(playerId);
    }

    /// <summary>
    /// Read only access for callers that are not about to save. Returns the
    /// instance the change tracker already holds, which after any ship operation
    /// in this request is the one that was just saved.
    /// </summary>
    public async Task<Ship_Database> GetShipAsync(Guid shipId)
    {
        await UpdateEnergyGeneration(shipId);
        return await m_repo.GetShipAsync(shipId);
    }

    /// <summary>
    /// Loads a ship for an operation that is about to modify and save it.
    ///
    /// Tracked ship rows are dropped first on purpose. Without that EF hands back
    /// the instance this request happened to read earlier, which can be older than
    /// whatever another request committed while we were waiting for the lock.
    /// Saving that instance would rewrite the whole ship document and silently
    /// undo the other request, which is the exact race the lock exists to prevent.
    ///
    /// Only ever call this from a ...Locked method, and only for the ship the
    /// current operation is about to save.
    /// </summary>
    private async Task<Ship_Database> ReloadShipLocked(Guid shipId)
    {
        m_repo.DiscardTrackedShips();
        await UpdateEnergyGeneration(shipId);
        return await m_repo.GetShipAsync(shipId);
    }

    private async Task<ShipInventory> ReloadInventoryLocked(string playerId)
    {
        m_repo.DiscardTrackedShips();
        return await m_repo.GetAsync(playerId);
    }

    #endregion

    // =========================================
    // CREATE SHIP (NOW WITH LIMIT CHECK)
    // =========================================

    public Task<Ship_Database> CreateShipAsync(string playerId, Ship inShip)
        => WithPlayerLockAsync(playerId, () => CreateShipLocked(playerId, inShip));

    private async Task<Ship_Database> CreateShipLocked(string playerId, Ship inShip)
    {
        var inventory = await ReloadInventoryLocked(playerId);

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
            Identity = inShip.Identity,
            State = inShip.State,
            GeneralData = inShip.GeneralData,
            Grid = inShip.ShipGrid,

            IsLocked = inShip.State.Locked,
            Items = new List<ShipItem>(),
        };

        inventory.Ships.Add(ship);

        await m_repo.AddShipAsync(ship);
        await m_repo.SaveAsync();

        return ship;
    }

    // =========================================
    // INCREASE SHIP CAPACITY
    // =========================================

    public Task AddShipSlotsAsync(string playerId, int amount)
        => WithPlayerLockAsync(playerId, () => AddShipSlotsLocked(playerId, amount));

    private async Task AddShipSlotsLocked(string playerId, int amount)
    {
        if (amount <= 0)
            return;

        var inventory = await ReloadInventoryLocked(playerId);

        inventory.NumOfShips += amount;

        await m_repo.SaveAsync();
    }

    public Task<bool> SelectActiveShipAsync(string playerId, Guid shipId)
        => WithPlayerLockAsync(playerId, () => SelectActiveShipLocked(playerId, shipId));

    private async Task<bool> SelectActiveShipLocked(string playerId, Guid shipId)
    {
        var inventory = await ReloadInventoryLocked(playerId);

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
    public Task UnlockShipSlotsAsync(Guid shipId)
        => WithShipLockAsync(shipId, () => UnlockShipSlotsLocked(shipId));

    private async Task UnlockShipSlotsLocked(Guid shipId)
    {
        var ship = await ReloadShipLocked(shipId);
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

    public Task LockShipSlotsAsync(Guid shipId)
        => WithShipLockAsync(shipId, () => LockShipSlotsLocked(shipId));

    private async Task LockShipSlotsLocked(Guid shipId)
    {
        var ship = await ReloadShipLocked(shipId);
        var grid = ship.Grid;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (grid.Get(x, y) == UNLOCKABLE)
                {
                    grid.Set(x, y, LOCKED);
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

    public Task<bool> TryUnlockSlotAsync(Guid shipId, int x, int y)
        => WithShipLockAsync(shipId, () => TryUnlockSlotLocked(shipId, x, y));

    private async Task<bool> TryUnlockSlotLocked(Guid shipId, int x, int y)
    {
        var ship = await ReloadShipLocked(shipId);

        if(ship.State.NumOfSlotToUnlock <= 0)
            return false;

        var grid = ship.Grid;

        if (grid.Get(x, y) != UNLOCKABLE)
            return false;

        grid.Set(x, y, EMPTY);
        ship.State.NumOfSlotToUnlock--;

        await m_repo.SaveAsync();
        if(ship.State.NumOfSlotToUnlock <= 0)
            await LockShipSlotsLocked(shipId);
        return true;
    }
    #endregion

    #region LockShip
    public Task UnlockShipInventoryAsync(Guid shipId)
        => WithShipLockAsync(shipId, () => UnlockShipInventoryLocked(shipId));

    private async Task UnlockShipInventoryLocked(Guid shipId)
    {
        var ship = await ReloadShipLocked(shipId);
        ship.IsLocked = false;
        await m_repo.SaveAsync();
    }

    public Task LockShipInventoryAsync(Guid shipId)
        => WithShipLockAsync(shipId, () => LockShipInventoryLocked(shipId));

    private async Task LockShipInventoryLocked(Guid shipId)
    {
        var ship = await ReloadShipLocked(shipId);
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
    public Task SendToMissionAsync(Guid shipId, Guid missionId)
        => WithShipLockAsync(shipId, () => SendToMissionLocked(shipId, missionId));

    private async Task SendToMissionLocked(Guid shipId, Guid missionId)
    {
        var ship = await ReloadShipLocked(shipId);
        ship.State.MissionID = missionId;
        ship.State.Traveling = true;
        await LockShipInventoryLocked(shipId);
    }

    /// <summary>
    /// Returns ship from mission and unlocks inventory
    /// </summary>
    /// <param name="shipId"></param>
    /// <returns></returns>
    public Task ReturnFromMissionAsync(Guid shipId)
        => WithShipLockAsync(shipId, () => ReturnFromMissionLocked(shipId));

    private async Task ReturnFromMissionLocked(Guid shipId)
    {
        var ship = await ReloadShipLocked(shipId);
        ship.State.Traveling = false;
        await UnlockShipInventoryLocked(shipId);
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
    public Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item, int x, int y)
        => WithShipLockAsync(shipId, () => TryPlaceItemLocked(shipId, item, x, y));

    private async Task<bool> TryPlaceItemLocked(Guid shipId, Item_Database item, int x, int y)
    {
        var ship = await ReloadShipLocked(shipId);

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

    public Task<bool> TryPlaceItemAsync(Guid shipId, Item_Database item)
        => WithShipLockAsync(shipId, () => TryPlaceItemAnywhereLocked(shipId, item));

    private async Task<bool> TryPlaceItemAnywhereLocked(Guid shipId, Item_Database item)
    {
        var ship = await ReloadShipLocked(shipId);
        if(item.IsEquipped)
            return false;
        if (ship.IsLocked)
            return false;

        for (int x = 0; x < ship.Grid.Width; x++)
        {
            for (int y = 0; y < ship.Grid.Height; y++)
            {
                if(CanPlaceItem(ship, item, x, y))
                    if(await TryPlaceItemLocked(shipId, item, x, y))
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
    public Task<bool> RemoveItemAsync(Guid shipId, Guid itemId)
        => WithShipLockAsync(shipId, () => RemoveItemLocked(shipId, itemId));

    private async Task<bool> RemoveItemLocked(Guid shipId, Guid itemId)
    {
        var ship = await ReloadShipLocked(shipId);
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

    #region Experience
    public Task AddExp(Guid shipId, int amount)
        => WithShipLockAsync(shipId, () => AddExpLocked(shipId, amount));

    private async Task AddExpLocked(Guid shipId, int amount)
    {
        if (amount <= 0)
            return;

        var ship = await ReloadShipLocked(shipId);

        if (ship.GeneralData.Level >= ship.GeneralData.MaxLevel)
        {
            ship.GeneralData.Level = ship.GeneralData.MaxLevel;
            ship.GeneralData.CurrentExperience = 0;

            await m_repo.SaveAsync();
            return;
        }

        ship.GeneralData.CurrentExperience += amount;

        //Level up
        while ( ship.GeneralData.Level < ship.GeneralData.MaxLevel &&
                ship.GeneralData.CurrentExperience >= ship.GeneralData.NextLevelExperience)
        {
            ship.GeneralData.CurrentExperience -=
                ship.GeneralData.NextLevelExperience;

            ship.GeneralData.Level++;

            ship.State.NumOfSlotToUnlock++;

            ship.GeneralData.NextLevelExperience =
                (int)BalanceDataProvider.CalculateXpForLevel(
                    ship.GeneralData.Level
                );
        }

        if (ship.GeneralData.Level >= ship.GeneralData.MaxLevel)
        {
            ship.GeneralData.Level = ship.GeneralData.MaxLevel;
            ship.GeneralData.CurrentExperience = 0;
        }

        await m_repo.SaveAsync();
        if(ship.State.NumOfSlotToUnlock > 0)
            // Already inside the lock, so the non reentrant semaphore must not be
            // taken again through the public method.
            await UnlockShipSlotsLocked(shipId);
    }
    #endregion

    #region Energy
    public Task AddEnergy(Guid shipId, float amount)
        => WithShipLockAsync(shipId, () => AddEnergyLocked(shipId, amount));

    private async Task AddEnergyLocked(Guid shipId, float amount)
    {
        var ship = await ReloadShipLocked(shipId);

        ship.GeneralData.CurrentEnergy = Math.Min(
            ship.GeneralData.CurrentEnergy + amount,
            ship.GeneralData.MaxEnergy
        );

        await m_repo.SaveAsync();
    }

    public Task<bool> UseEnergy(Guid shipId, float amount)
        => WithShipLockAsync(shipId, () => UseEnergyLocked(shipId, amount));

    private async Task<bool> UseEnergyLocked(Guid shipId, float amount)
    {
        var ship = await ReloadShipLocked(shipId);

        if(ship.GeneralData.CurrentEnergy > amount)
        {
            ship.GeneralData.CurrentEnergy -= amount;
            await m_repo.SaveAsync();
            return true;
        }

        return false;
    }

    public Task UpdateEnergyRegenSpeed(Guid shipId, float energyRegenSpeed)
        => WithShipLockAsync(shipId, () => UpdateEnergyRegenSpeedLocked(shipId, energyRegenSpeed));

    private async Task UpdateEnergyRegenSpeedLocked(Guid shipId, float energyRegenSpeed)
    {
        var ship = await ReloadShipLocked(shipId);
        ship.GeneralData.EnergyRegenSpeed = BalanceDataProvider.CalculateEnergyGeneration(energyRegenSpeed);

        await m_repo.SaveAsync();
    }

    public Task UpdateEnergyMaximum(Guid shipId, float energyMaximum)
        => WithShipLockAsync(shipId, () => UpdateEnergyMaximumLocked(shipId, energyMaximum));

    private async Task UpdateEnergyMaximumLocked(Guid shipId, float energyMaximum)
    {
        var ship = await ReloadShipLocked(shipId);
        ship.GeneralData.MaxEnergy = BalanceDataProvider.CalculateEnergyCap(energyMaximum);

        await m_repo.SaveAsync();
    }

    private async Task UpdateEnergyGeneration(Guid shipId)
    {
        var ship = await m_repo.GetShipAsync(shipId);

        var now = DateTime.UtcNow;
        var seconds = (now - ship.GeneralData.LastEnergyUpdate).TotalSeconds;

        var generated = (float)(seconds * ship.GeneralData.EnergyRegenSpeed);

        ship.GeneralData.CurrentEnergy = Math.Min(
            ship.GeneralData.CurrentEnergy + generated,
            ship.GeneralData.MaxEnergy
        );
    }

    private async Task UpdateEnergyGeneration(string playerId)
    {
        var shipInventory = await m_repo.GetAsync(playerId);
        foreach(var ship in shipInventory.Ships)
        {
            await UpdateEnergyGeneration(ship.Identity.Id);
        }
    }
    #endregion
}