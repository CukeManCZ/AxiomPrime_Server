using AxiomPrime_Metadata.General;

public class ItemStat_Database
{
    public StatIdentity Identity { get; set; } = new();
    public StatGeneralData GeneralData { get; set; } = new();

    public float Weight { get; set; }

    public string Name
    {
        get => Identity?.Id.ToString() ?? string.Empty;
        set
        {
            Identity ??= new StatIdentity();
            if (Enum.TryParse<StatId>(value, true, out var statId))
            {
                Identity.Id = statId;
            }
        }
    }

    public string StatType
    {
        get => Identity?.Id.ToString() ?? string.Empty;
        set { }
    }

    public float Value
    {
        get => GeneralData?.Value ?? 0f;
        set
        {
            GeneralData ??= new StatGeneralData();
            GeneralData.Value = value;
        }
    }

    public bool IsPercentage
    {
        get => Identity?.IsPercentage ?? false;
        set
        {
            Identity ??= new StatIdentity();
            Identity.IsPercentage = value;
        }
    }
}