namespace UnsecuredAPIKeys.WebAPI.Services
{
    public interface IDiscordRoleService
    {
        Task<int> GetUserRateLimitAsync(string discordId);
        Task UpdateUserRolesAsync(string discordId);
        Task<List<string>> GetUserRolesAsync(string discordId);
    }
}
