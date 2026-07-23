using Microsoft.EntityFrameworkCore;

public class EnergyRepository
{
    private readonly GameDbContext m_db;

    public EnergyRepository(GameDbContext db)
    {
        m_db = db;
    }

    public async Task<Energy> GetAsync(string playerID)
    {
        var entity = await m_db.Energies
            .FirstOrDefaultAsync(x => x.PlayerID == playerID);

        if (entity != null)
            return entity;

        entity = new Energy
        {
            PlayerID = playerID,
            CurrentEnergy = 100,
            MaxEnergy = 100,
            RegenSpeed = 1f,
            LastUpdate =  DateTime.UtcNow
        };

        m_db.Energies.Add(entity);
        await m_db.SaveChangesAsync();

        return entity;
    }

    public async Task SaveAsync(Energy energy)
    {
        m_db.Energies.Update(energy);
        await m_db.SaveChangesAsync();
    }
}