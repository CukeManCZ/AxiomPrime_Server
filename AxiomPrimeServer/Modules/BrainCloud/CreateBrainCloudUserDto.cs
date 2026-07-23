public class CreateBrainCloudUserDTO
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string UserName { get; set; }
    public string? NotificationTemplateId { get; set; }
}