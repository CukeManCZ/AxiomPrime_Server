public class BrainCloudService
{
    private readonly BrainCloudClient _bc;

    public BrainCloudService(BrainCloudClient bc)
    {
        _bc = bc;
    }

    // --------------------------------------------------
    // BOT / GLOBAL ENTITY
    // --------------------------------------------------
    public Task<string> GetBots()
    {
        return _bc.CallAsync(
            "globalEntity",
            "GET_SYSTEM_ENTITY_LIST_COUNT",
            new
            {
                where = new { entityType = "bot" }
            });
    }

    // --------------------------------------------------
    // CREATE USER
    // --------------------------------------------------
    public Task<string> CreateUserEmailPassword(
        string email,
        string password,
        string userName,
        string? notificationTemplateId = null)
    {
        return _bc.CallAsync(
            "user",
            "SYS_CREATE_USER_EMAIL_PASSWORD",
            new
            {
                externalId = email,
                password = password,
                userName = userName,
                notificationTemplateId = notificationTemplateId
            });
    }

    // --------------------------------------------------
    // DELETE USER
    // --------------------------------------------------
    public Task<string> DeleteUser(string profileId, bool deleteChildren = true)
    {
        return _bc.CallAsync(
            "user",
            "SYS_DELETE_USER",
            new
            {
                profileId = profileId,
                optionsJson = new
                {
                    deleteChildren = deleteChildren
                }
            });
    }

    // --------------------------------------------------
    // GET USER INFO (TYPED)
    // --------------------------------------------------
    public Task<BrainCloudUserData?> GetUserInfo(string profileId)
    {
        return _bc.CallAsync<BrainCloudUserData>(
            "user",
            "SYS_GET_USER_INFO",
            new
            {
                profileId = profileId
            });
    }
}