public class ItemStatDto
{
    public string Name { get; set; } = default!;
    public string StatType { get; set; } = default!;
    public float Value { get; set; }
    public bool IsPercentage { get; set; }
}