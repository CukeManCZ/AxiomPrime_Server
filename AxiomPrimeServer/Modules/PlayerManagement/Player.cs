using Microsoft.EntityFrameworkCore;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Id), IsUnique = true)]
public class Player
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email {get; set; }
    public Currencies? Currencies {get; set;}
    public Experience? Experience {get; set;}
}