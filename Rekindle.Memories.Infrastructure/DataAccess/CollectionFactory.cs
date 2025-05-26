using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Rekindle.Memories.Infrastructure.DataAccess;

public class CollectionFactory : ICollectionFactory
{
    private readonly IMongoDatabase _database;

    public CollectionFactory(IOptions<FaceDatabaseConfig> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _database = mongoClient.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        var collectionName = typeof(T).Name;
        return _database.GetCollection<T>(collectionName);
    }
}