
using System.ComponentModel.DataAnnotations;

public class Experience
{
    [Key]
    public required string PlayerID {get; set;}
    public required int Level {get; set;}
    public required int CurrentExperience {get; set;}
    public required int NextLevelExperience {get; set;}
}