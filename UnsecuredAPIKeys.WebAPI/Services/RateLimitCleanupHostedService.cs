using Microsoft.Extensions.Hosting;

namespace UnsecuredAPIKeys.WebAPI.Services
{
    public class RateLimitCleanupHostedService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
