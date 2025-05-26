using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Infrastructure.DataAccess;

/// <summary>
/// MongoDB database context for Memories domain
/// </summary>
public class MemoriesDbContext : IMemoriesDbContext
{
    private readonly IMongoDatabase _database;

    public MemoriesDbContext(IMongoClient mongoClient, IOptions<MemoriesDbConfig> config)
    {
        _database = mongoClient.GetDatabase(config.Value.DatabaseName);
    }

    /// <inheritdoc />
    public IMongoDatabase Database => _database;

    /// <inheritdoc />
    public IMongoCollection<Memory> Memories => _database.GetCollection<Memory>("memories");

    /// <inheritdoc />
    public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");

    /// <inheritdoc />
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");

    /// <inheritdoc />
    public IMongoCollection<Group> Groups => _database.GetCollection<Group>("groups");

    /// <inheritdoc />
    public IMongoCollection<T> GetCollection<T>(string? name = null)
    {
        var collectionName = name ?? typeof(T).Name.ToLowerInvariant();
        return _database.GetCollection<T>(collectionName);
    }
}
