public class BrainCloudUserData
{
    public required string EmailAddress { get; set; }
    public required string PlayerName { get; set; }
    public required string ProfileId { get; set; }
    public required string CountryCode { get; set; }
    public float? TimeZoneOffset { get; set; }
    public object? SummaryFriendData { get; set; }
}