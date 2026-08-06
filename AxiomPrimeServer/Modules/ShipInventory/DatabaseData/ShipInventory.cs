using System.ComponentModel.DataAnnotations;

public class ShipInventory
{
    [Key]
    public required string PlayerId {get; set;}
    public Guid ActiveShip {get; set;}
    public List<Ship_Database> Ships {get; set;} = new();
    public required int NumOfShips {get; set;}
}