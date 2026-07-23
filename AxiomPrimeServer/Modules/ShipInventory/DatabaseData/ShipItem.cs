using System.ComponentModel.DataAnnotations;

public class ShipItem
{
    [Key]
    public Guid Id { get; set; }

    public Guid ShipId { get; set; }
    public Ship Ship { get; set; } = default!;

    // reference to real item
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = default!;

    // position inside ship grid
    public int X { get; set; }
    public int Y { get; set; }
}
