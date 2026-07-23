using Microsoft.EntityFrameworkCore;

public class CurrenciesRepository
{
    private readonly GameDbContext m_db;

    public CurrenciesRepository(GameDbContext db)
    {
        m_db = db;
    }

    public async Task<Currencies> GetAsync(string playerId)
    {
        var entity = await m_db.Currencies
            .FirstOrDefaultAsync(x => x.PlayerID == playerId);

        if (entity != null)
            return entity;

        entity = new Currencies
        {
            PlayerID = playerId,
            Credits = 0,
            PremiumCredits = 0,
            Scrap = 0
        };

        m_db.Currencies.Add(entity);
        await m_db.SaveChangesAsync();

        return entity;
    }

    public async Task SaveAsync(Currencies currencies)
    {
        m_db.Currencies.Update(currencies);
        await m_db.SaveChangesAsync();
    }
}