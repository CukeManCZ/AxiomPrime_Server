using System.ComponentModel.DataAnnotations;

public class Item
{
    public Guid Id { get; set; }

    public string ItemName { get; set; } = default!;

    public float Power { get; set; }
    public int Level { get; set; }
    public int Price { get; set; }

    public bool IsEquipped { get; set; }

    public ItemGridData Size { get; set; } = default!;
    public ItemStatsData StatsData { get; set; } = new();
}