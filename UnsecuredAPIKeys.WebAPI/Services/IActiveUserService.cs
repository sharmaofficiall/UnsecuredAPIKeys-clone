namespace UnsecuredAPIKeys.WebAPI.Services
{
    public interface IActiveUserService
    {
        int ActiveUserCount { get; }
        Task UserConnectedAsync(string connectionId);
        Task UserDisconnectedAsync(string connectionId);
        Task BroadcastUserCountAsync();
        Task ValidateConnectionsAsync();
        Task UpdateLastSeenAsync(string connectionId);
    }
}
