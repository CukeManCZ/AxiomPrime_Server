using Microsoft.EntityFrameworkCore;

public class MissionRepository
{
    private readonly GameDbContext _db;

    public MissionRepository(GameDbContext db)
        => _db = db;

    public async Task<List<Mission_Database>> GetAsync(string playerId)
        => await _db.Missions
            .Where(x => x.PlayerId == playerId)
            .ToListAsync();

    public async Task AddAsync(Mission_Database mission)
    {
        _db.Missions.Add(mission);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(Mission_Database mission)
    {
        _db.Missions.Remove(mission);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Mission_Database mission)
    {
        _db.Missions.Update(mission);
        await _db.SaveChangesAsync();
    }
}