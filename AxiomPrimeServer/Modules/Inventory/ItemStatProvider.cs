using AxiomPrime_Generics;
using AxiomPrime_Metadata.General;

public class DatabaseItemStatProvider : IStatProvider
{
    private readonly Item_Database m_item;

    public DatabaseItemStatProvider(Item_Database item)
    {
        m_item = item;
    }

    public IEnumerable<GenericStat> GetStats()
    {
        var stats = m_item.StatsData.GetStats();
        foreach (var stat in stats)
        {
            yield return new GenericStat
            {
                Identity = stat.Identity,
                GeneralData = stat.GeneralData,
            };
        }
    }
}