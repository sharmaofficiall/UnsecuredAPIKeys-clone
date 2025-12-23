using UnsecuredAPIKeys.Data.Models;

namespace UnsecuredAPIKeys.WebAPI.Services
{
    public interface IDiscordService
    {
        Task<DiscordUser?> GetOrCreateUserAsync(string discordId, string accessToken, string refreshToken, DateTime tokenExpiresAt, string? ipAddress = null);
        Task<bool> VerifyServerMembershipAsync(string discordId, string accessToken);
        Task<DiscordUser?> GetUserByIdAsync(string discordId);
        Task<int?> GetUserRateLimitAsync(string discordId);
        Task RefreshUserTokenIfNeededAsync(DiscordUser user);
        Task<string> GetAuthorizationUrlAsync();
    }
}
