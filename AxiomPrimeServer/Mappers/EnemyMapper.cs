using AxiomPrime.Models.Enemies;
using AxiomPrime_DTOs.Inventory;
using AxiomPrime_Metadata.Enemy;
using AxiomPrime.Services;
using AxiomPrime_Metadata.General;

public static class EnemyMapper
{
    public static EnemyDto ToDto(Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);

        var genericStats = enemy.Stats?
            .Select(s => new GenericStat { Identity = s.Identity, GeneralData = s.GeneralData })
            .ToList() ?? new List<GenericStat>();

        var shipStats = StatSummer.GetShipStats(genericStats);

        return new EnemyDto
        {
            Identity = enemy.Identity,
            GeneralData = enemy.GeneralData,
            Stats = new StatsDataDto
            {
                Data = shipStats.GetStats()
                    .Select(stat => new StatDto
                    {
                        Identity = stat.Identity,
                        GeneralData = stat.GeneralData
                    })
                    .ToList()
            }
        };
    }

    public static List<EnemyDto> ToDto(List<Enemy> enemies)
    {
        var result = new List<EnemyDto>();
        foreach (var e in enemies)
            result.Add(ToDto(e));

        return result;
    }
}
