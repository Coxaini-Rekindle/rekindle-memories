using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rekindle.Memories.Infrastructure.DataAccess;
using Rekindle.Memories.Infrastructure.Messaging;

namespace Rekindle.Memories.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRebusMessageBus(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddMongoDb(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure MongoDB
        var memoriesDbSection = configuration.GetSection(MemoriesDbConfig.MemoriesDb);
        services.Configure<MemoriesDbConfig>(memoriesDbSection);
        
        // Register MongoDB services
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<MemoriesDbConfig>>().Value;
            return new MongoClient(config.ConnectionString);
        });
        
        services.AddScoped<IMongoDatabase>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<MemoriesDbConfig>>().Value;
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(config.DatabaseName);
        });
        
        // Register database context
        services.AddScoped<IMemoriesDbContext, MemoriesDbContext>();
        
        return services;
    }
}