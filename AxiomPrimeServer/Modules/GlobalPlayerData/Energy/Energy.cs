using System.ComponentModel.DataAnnotations;

public class Energy
{
    [Key]
    public required string PlayerID {get; set;}
    public required float CurrentEnergy {get; set;}
    public required float RegenSpeed {get; set;}
    public required float MaxEnergy {get; set;}
    
    public DateTime LastUpdate { get; set; }
}