using System.ComponentModel.DataAnnotations;

public class Currencies
{
    [Key]
    public required string PlayerID {get; set;}
    public required int Credits {get; set;}
    public required int PremiumCredits {get; set;}
    public required int Scrap {get; set;}
}