using Utilities.DataStructures;

public class ItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public int Level { get; set; }

    public bool Equipped { get; set; }

    public CustomGridDto<bool> Size { get; set; } = default!;

    public ItemStatsDataDto StatsData { get; set; } = new();
}