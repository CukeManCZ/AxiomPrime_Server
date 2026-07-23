using System.Security.Claims;

namespace Utilities.AuthorizationTools;

public static class ClaimsPrincipalExtensions
{
    public static string? GetProfileId(this ClaimsPrincipal user)
    {
        return user.FindFirst("profileId")?.Value;
    }

    public static bool TryGetProfileId(this ClaimsPrincipal user, out string profileId)
    {
        profileId = user.FindFirst("profileId")?.Value ?? string.Empty;
        return !string.IsNullOrWhiteSpace(profileId);
    }
}