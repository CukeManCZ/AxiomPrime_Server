using System.Text.Json;

public class ItemStatsData
{
    public string Data { get; set; } = "[]";

    public void SetStats(List<ItemStat> itemStats)
    {
        Data = JsonSerializer.Serialize(itemStats);
    }

    public List<ItemStat> GetStats()
    {
        if (string.IsNullOrWhiteSpace(Data))
            return new List<ItemStat>();

        return JsonSerializer.Deserialize<List<ItemStat>>(Data) ?? new List<ItemStat>();
    }
}