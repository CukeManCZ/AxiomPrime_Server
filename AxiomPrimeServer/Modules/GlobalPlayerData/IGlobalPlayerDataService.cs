public interface IGlobalPlayerDataService
{
    Task<GlobalPlayerDataDTO> GetAsync(string playerId);

    Task AddEnergy(string playerId, float amount);
    Task<bool> UseEnergy(string playerId, float amount);

    Task AddMoney(string playerId, int amount);
    Task<bool> UseMoney(string playerId, int amount);

    Task AddPremium(string playerId, int amount);
    Task<bool> UsePremium(string playerId, int amount);

    Task AddScraps(string playerId, int amount);
    Task<bool> UseScraps(string playerId, int amount);

    Task AddExp(string playerId, int amount);
}