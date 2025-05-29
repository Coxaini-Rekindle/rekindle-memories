using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Rekindle.Authentication;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Storage.Interfaces;
using Rekindle.Memories.Infrastructure.DataAccess;
using Rekindle.Memories.Infrastructure.Messaging;
using Rekindle.Memories.Infrastructure.Repositories.Groups;
using Rekindle.Memories.Infrastructure.Repositories.Memories;
using Rekindle.Memories.Infrastructure.Services;
using Rekindle.Memories.Infrastructure.Storage;

namespace Rekindle.Memories.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRebusMessageBus(configuration);
        services.AddRepositories();
        services.AddJwtAuth(configuration);
        services.AddFileStorage(configuration);
        
        return services;
    }    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITransactionManager, MongoTransactionManager>();

        return services;
    }
    
    private static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));

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
        
        BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
        
        // Register database context
        services.AddScoped<IMemoriesDbContext, MemoriesDbContext>();
        
        services.AddHostedService<DatabaseConfigurationService>();
          return services;
    }
}