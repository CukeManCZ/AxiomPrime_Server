using AxiomPrime_DTOs.GlobalData;

public interface IGlobalPlayerDataService
{
    Task<GlobalPlayerDataDTO> GetAsync(string playerId);

    Task AddMoney(string playerId, int amount);
    Task<bool> UseMoney(string playerId, int amount);

    Task AddPremium(string playerId, int amount);
    Task<bool> UsePremium(string playerId, int amount);

    Task AddScraps(string playerId, int amount);
    Task<bool> UseScraps(string playerId, int amount);

    Task AddExp(string playerId, int amount);

    /// <summary>
    /// Grants all rewards of a single mission in one atomic operation, so that no
    /// other operation for the same player can read a half-applied reward state.
    /// </summary>
    Task AddMissionRewards(
        string playerId,
        int credits,
        int premiumCredits,
        int experience,
        int scraps);
}