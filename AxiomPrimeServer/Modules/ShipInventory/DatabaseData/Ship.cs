using System.ComponentModel.DataAnnotations;

public class Ship
{
    [Key]
    public Guid Id { get; set; }

    public string ShipInventoryId { get; set; } = default!;
    public ShipInventory ShipInventory { get; set; } = default!;

    public bool IsLocked { get; set; }

    public int XOrigin { get; set; }
    public int YOrigin { get; set; }

    // GRID STATE (persistent)
    public ShipGrid Grid { get; set; } = default!;

    // Items inside ship
    public List<ShipItem> Items { get; set; } = new();
}