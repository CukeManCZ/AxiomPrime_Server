using System.Text.Json;

public class ItemStatsData
{
    public string Data { get; set; } = "[]";

    public void SetStats(List<ItemStat_Database> itemStats)
    {
        Data = JsonSerializer.Serialize(itemStats);
    }

    public List<ItemStat_Database> GetStats()
    {
        if (string.IsNullOrWhiteSpace(Data))
            return new List<ItemStat_Database>();

        return JsonSerializer.Deserialize<List<ItemStat_Database>>(Data) ?? new List<ItemStat_Database>();
    }
}