using Microsoft.EntityFrameworkCore;

public class ExperienceRepository
{
    private readonly GameDbContext m_db;

    public ExperienceRepository(GameDbContext db)
    {
        m_db = db;
    }

    public async Task<Experience> GetAsync(string playerID)
    {
        var entity = await m_db.Experiences
            .FirstOrDefaultAsync(x => x.PlayerID == playerID);

        if (entity != null)
            return entity;

        entity = new Experience
        {
            PlayerID = playerID,
            Level = 1,
            CurrentExperience = 0,
            NextLevelExperience = 100
        };

        m_db.Experiences.Add(entity);
        await m_db.SaveChangesAsync();

        return entity;
    }

    public async Task SaveAsync(Experience experience)
    {
        m_db.Experiences.Update(experience);
        await m_db.SaveChangesAsync();
    }
}