using System.ComponentModel.DataAnnotations;

public class Inventory
{
    [Key]
    public required string PlayerId { get; set; }
    public List<Item_Database> Items { get; set; } = new();
    public required int numOfItems {get; set;}
}