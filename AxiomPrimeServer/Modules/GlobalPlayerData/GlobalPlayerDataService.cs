using AxiomPrime.Services;
using AxiomPrime_DTOs.GlobalData;

public class GlobalPlayerDataService : IGlobalPlayerDataService
{
    private readonly ExperienceRepository m_experience_repository;
    private readonly CurrenciesRepository m_currencies_repository;
    private readonly PlayerLockProvider m_playerLockProvider;

    public GlobalPlayerDataService(
        ExperienceRepository experienceRepository,
        CurrenciesRepository currenciesRepository,
        PlayerLockProvider playerLockProvider
    )
    {
        m_experience_repository = experienceRepository;
        m_currencies_repository = currenciesRepository;
        m_playerLockProvider = playerLockProvider;
    }

    public Task InitializePlayer(string playerId)
    => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var exp = await m_experience_repository.GetAsync(playerId); 
            exp.Level = 1;
            exp.NextLevelExperience = (int) BalanceDataProvider.CalculateXpForLevel(1);
            exp.CurrentExperience = 0;

            var currencies = await m_currencies_repository.GetAsync(playerId);
            currencies.Credits = 100;
            currencies.PremiumCredits = 999;
            currencies.Scrap = 100;

            await m_experience_repository.SaveAsync(exp);
            await m_currencies_repository.SaveAsync(currencies);
        });

    // =========================================================
    #region GET FULL PLAYER STATE
    // =========================================================

    public Task<GlobalPlayerDataDTO> GetAsync(string playerId)
    => m_playerLockProvider.WithLock(playerId, async () =>
    {
        var exp = await m_experience_repository.GetAsync(playerId);
        var curr = await m_currencies_repository.GetAsync(playerId);
        return new GlobalPlayerDataDTO
        {
            Level = exp.Level,
            CurrentExp = exp.CurrentExperience,
            NextLevelExp = exp.NextLevelExperience,

            Credits = curr.Credits,
            PremiumCredits = curr.PremiumCredits,
            Scraps = curr.Scrap
        };
    });
    #endregion

    // =========================================================
    #region CURRENCY SYSTEM
    // =========================================================

    public Task AddMoney(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);
            curr.Credits += amount;
            await m_currencies_repository.SaveAsync(curr);
        });

    public Task<bool> UseMoney(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);

            if (curr.Credits < amount)
                return false;

            curr.Credits -= amount;

            await m_currencies_repository.SaveAsync(curr);
            return true;
        });

    public Task AddPremium(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);
            curr.PremiumCredits += amount;
            await m_currencies_repository.SaveAsync(curr);
        });

    public Task<bool> UsePremium(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);

            if (curr.PremiumCredits < amount)
                return false;

            curr.PremiumCredits -= amount;

            await m_currencies_repository.SaveAsync(curr);
            return true;
        });

    public Task AddScraps(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);
            curr.Scrap += amount;
            await m_currencies_repository.SaveAsync(curr);
        });

    public Task<bool> UseScraps(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var curr = await m_currencies_repository.GetAsync(playerId);

            if (curr.Scrap < amount)
                return false;

            curr.Scrap -= amount;

            await m_currencies_repository.SaveAsync(curr);
            return true;
        });

    #endregion

    // =========================================================
    #region EXPERIENCE SYSTEM
    // =========================================================

    public Task AddExp(string playerId, int amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var exp = await m_experience_repository.GetAsync(playerId);
    
            exp.CurrentExperience += amount;

            //Level up
            while(exp.CurrentExperience >= exp.NextLevelExperience)
            {
                int expAboveLevel = exp.CurrentExperience - exp.NextLevelExperience;
                exp.Level++;
                exp.NextLevelExperience = (int) BalanceDataProvider.CalculateXpForLevel(exp.Level);
                exp.CurrentExperience = expAboveLevel;
            }

            await m_experience_repository.SaveAsync(exp);
        });
    
    #endregion
}