using System.Collections.Generic;
using System.Linq;
using AxiomPrime.Models.Fight;
using AxiomPrime.Models.Stats;
using AxiomPrime.Services;
using AxiomPrime_Shared.Stats;

namespace AxiomPrimeServer.Services
{
    /// <summary>
    /// Adapter that provides stats from a database Item (server-side).
    /// Implements IStatProvider so it can be used with StatSummer.
    /// </summary>
    public class DatabaseItemStatProvider : IStatProvider
    {
        private readonly Item_Database _item;

        public DatabaseItemStatProvider(Item_Database item)
        {
            _item = item;
        }

        public IEnumerable<ProvidedStat> GetStats()
        {
            var stats = _item.StatsData.GetStats();
            foreach (var stat in stats)
            {
                yield return new ProvidedStat
                {
                    Id = stat.Identity.Id,
                    Value = stat.GeneralData.Value,
                    Weight = stat.Weight,
                    IsPercentage = stat.Identity.IsPercentage
                };
            }
        }
    }

    /// <summary>
    /// Extension methods for calculating ship stats using StatSummer.
    /// </summary>
    public static class ShipStatExtensions
    {
        /// <summary>
        /// Gets equipped item providers from a ship.
        /// </summary>
        public static IEnumerable<IStatProvider> GetEquippedItemProviders(this Ship_Database ship)
        {
            return ship.Items
                .Select(si => si.Item)
                .Where(item => item.IsEquipped)
                .Select(item => (IStatProvider)new DatabaseItemStatProvider(item));
        }

        /// <summary>
        /// Calculates summed ship stats using StatSummer.
        /// </summary>
        public static ShipStats CalculateStats(this Ship_Database ship, StatService statService)
        {
            var summer = new StatSummer(statService);
            return summer.SumStats(GetEquippedItemProviders(ship));
        }
    }
}
