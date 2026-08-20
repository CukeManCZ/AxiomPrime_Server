using AxiomPrime_DTOs.GlobalData;

public class GlobalPlayerDataAPI
{
    private readonly GlobalPlayerDataService m_globalPlayerDataService;
    private readonly EventBus m_eventBus;

    public GlobalPlayerDataAPI(GlobalPlayerDataService globalPlayerDataService, EventBus eventBus)
    {
        m_globalPlayerDataService = globalPlayerDataService;
        m_eventBus = eventBus;
    }

    public Task InitializePlayer(string playerId)
        => m_globalPlayerDataService.InitializePlayer(playerId);

    // =========================================================
    #region READ
    // =========================================================

    public Task<GlobalPlayerDataDTO> GetAsync(string playerId)
        => m_globalPlayerDataService.GetAsync(playerId);

    #endregion

    // =========================================================
    #region CURRENCY
    // =========================================================

    public async Task AddMoney(string playerId, int amount)
    {
        await m_globalPlayerDataService.AddMoney(playerId, amount);
        var playerGlobalDataDTO = await m_globalPlayerDataService.GetAsync(playerId);

        await m_eventBus.Publish(new CurrencyDataUpdated()
        {
            PlayerId = playerId,
            Credits = playerGlobalDataDTO.Credits,
            PremiumCredits = playerGlobalDataDTO.PremiumCredits,
            Scraps = playerGlobalDataDTO.Scraps
        });
    }

    public Task<bool> UseMoney(string playerId, int amount)
        => m_globalPlayerDataService.UseMoney(playerId, amount);

    public Task AddPremium(string playerId, int amount)
        => m_globalPlayerDataService.AddPremium(playerId, amount);

    public Task<bool> UsePremium(string playerId, int amount)
        => m_globalPlayerDataService.UsePremium(playerId, amount);

    public Task AddScraps(string playerId, int amount)
        => m_globalPlayerDataService.AddScraps(playerId, amount);

    public Task<bool> UseScraps(string playerId, int amount)
        => m_globalPlayerDataService.UseScraps(playerId, amount);

    #endregion

    // =========================================================
    #region EXPERIENCE
    // =========================================================

    public Task AddExp(string playerId, int amount)
        => m_globalPlayerDataService.AddExp(playerId, amount);

    #endregion
}