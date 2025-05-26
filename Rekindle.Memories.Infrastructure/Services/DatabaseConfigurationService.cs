using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rekindle.Memories.Infrastructure.DataAccess;
using Rekindle.Memories.Infrastructure.DataAccess.Configuration;

namespace Rekindle.Memories.Infrastructure.Services;

/// <summary>
/// Background service to configure MongoDB on application startup
/// </summary>
public class DatabaseConfigurationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseConfigurationService> _logger;

    public DatabaseConfigurationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseConfigurationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuring MongoDB database...");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IMemoriesDbContext>();
            
            await MongoDbConfiguration.ConfigureAsync(dbContext.Database);
            
            _logger.LogInformation("MongoDB database configuration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure MongoDB database");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
