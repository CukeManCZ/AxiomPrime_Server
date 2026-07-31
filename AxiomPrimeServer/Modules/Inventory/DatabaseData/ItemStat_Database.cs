using AxiomPrime_Metadata.General;

public class ItemStat_Database
{
    public StatIdentity Identity { get; set; } = new();
    public StatGeneralData GeneralData { get; set; } = new();

    public float Weight { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
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

    [System.Text.Json.Serialization.JsonIgnore]
    public string StatType
    {
        get => Identity?.Type.ToString() ?? string.Empty;
        set
        {
            Identity ??= new StatIdentity();
            if (Enum.TryParse<StatType>(value, true, out var statType))
            {
                Identity.Type = statType;
            }
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public float Value
    {
        get => GeneralData?.Value ?? 0f;
        set
        {
            GeneralData ??= new StatGeneralData();
            GeneralData.Value = value;
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
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