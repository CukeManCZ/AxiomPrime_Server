using AxiomPrime.Services;
using AxiomPrime_Generics;
using AxiomPrime_Metadata.General;

/// <summary>
/// Extension methods for calculating ship stats using StatSummer.
/// </summary>
public class ShipStatProvider : IStatProvider
{
    private readonly Ship_Database m_ship;
    
    public ShipStatProvider(Ship_Database ship)
    {
        m_ship = ship;
    }

    public IEnumerable<GenericStat> GetStats()
    {
        return StatSummer.SumStats(GetEquippedItemProviders());
    }

    /// <summary>
    /// Gets equipped item providers from a ship.
    /// </summary>
    private IEnumerable<IStatProvider> GetEquippedItemProviders()
    {
        return m_ship.Items
            .Select(si => si.Item)
            .Where(item => item.IsEquipped)
            .Select(item => (IStatProvider)new DatabaseItemStatProvider(item));
    }
}