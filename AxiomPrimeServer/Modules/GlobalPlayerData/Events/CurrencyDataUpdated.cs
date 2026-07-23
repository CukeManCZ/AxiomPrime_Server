public class CurrencyDataUpdated : IEvent
{
    public required string PlayerId { get; set; }
    public int Credits {get; set;}
    public int PremiumCredits {get; set;}
    public int Scraps {get; set;}
}