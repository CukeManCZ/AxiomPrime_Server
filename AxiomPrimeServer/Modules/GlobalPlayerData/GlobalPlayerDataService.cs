using AxiomPrime.Services;
using AxiomPrime_DTOs.GlobalData;

public class GlobalPlayerDataService : IGlobalPlayerDataService
{
    private readonly ExperienceRepository m_experience_repository;
    private readonly EnergyRepository m_energy_repository;
    private readonly CurrenciesRepository m_currencies_repository;
    private readonly PlayerLockProvider m_playerLockProvider;

    public GlobalPlayerDataService(
        ExperienceRepository experienceRepository,
        EnergyRepository energyRepository,
        CurrenciesRepository currenciesRepository,
        PlayerLockProvider playerLockProvider
    )
    {
        m_experience_repository = experienceRepository;
        m_energy_repository = energyRepository;
        m_currencies_repository = currenciesRepository;
        m_playerLockProvider = playerLockProvider;
    }

    // =========================================================
    #region GET FULL PLAYER STATE
    // =========================================================

    public Task<GlobalPlayerDataDTO> GetAsync(string playerId)
    => m_playerLockProvider.WithLock(playerId, async () =>
    {
        var exp = await m_experience_repository.GetAsync(playerId);
        var energy = await m_energy_repository.GetAsync(playerId);
        var curr = await m_currencies_repository.GetAsync(playerId);
        await UpdateEnergyGeneration(energy);
        return new GlobalPlayerDataDTO
        {
            Level = exp.Level,
            CurrentExp = exp.CurrentExperience,
            NextLevelExp = exp.NextLevelExperience,

            Energy = energy.CurrentEnergy,
            MaxEnergy = energy.MaxEnergy,
            EnergyRegen = energy.RegenSpeed,

            Credits = curr.Credits,
            PremiumCredits = curr.PremiumCredits,
            Scraps = curr.Scrap
        };
    });
    #endregion

    // =========================================================
    #region ENERGY SYSTEM
    // =========================================================

    public Task AddEnergy(string playerId, float amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var energy = await m_energy_repository.GetAsync(playerId);

            await UpdateEnergyGeneration(energy);

            energy.CurrentEnergy = Math.Min(
                energy.CurrentEnergy + amount,
                energy.MaxEnergy
            );

            await m_energy_repository.SaveAsync(energy);
        });

    public Task<bool> UseEnergy(string playerId, float amount)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var energy = await m_energy_repository.GetAsync(playerId);

            await UpdateEnergyGeneration(energy);

            if (energy.CurrentEnergy < amount)
                return false;

            energy.CurrentEnergy -= amount;

            await m_energy_repository.SaveAsync(energy);
            return true;
        });

    public Task<bool> UpdateEnergyRegen(string playerId, float energyRegenSpeed)
        => m_playerLockProvider.WithLock(playerId, async () =>
        {
            var energy = await m_energy_repository.GetAsync(playerId);

            energy.RegenSpeed = energyRegenSpeed;

            await m_energy_repository.SaveAsync(energy);
            return true;
        });

    private async Task UpdateEnergyGeneration(Energy energy)
    {
        var now = DateTime.UtcNow;
        var seconds = (now - energy.LastUpdate).TotalSeconds;

        var generated = (float)(seconds * energy.RegenSpeed);

        energy.CurrentEnergy = Math.Min(
            energy.CurrentEnergy + generated,
            energy.MaxEnergy
        );

        energy.LastUpdate = now;
        await m_energy_repository.SaveAsync(energy);
    }

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
    
    #endregion
}