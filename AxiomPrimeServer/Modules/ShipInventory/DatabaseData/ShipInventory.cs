using System.ComponentModel.DataAnnotations;

public class ShipInventory
{
    [Key]
    public required string PlayerId {get; set;}
    public List<Ship> Ships {get; set;} = new();
    public required int NumOfShips {get; set;}
}