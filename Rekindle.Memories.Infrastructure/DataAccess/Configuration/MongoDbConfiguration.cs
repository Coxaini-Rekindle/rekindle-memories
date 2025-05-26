using MongoDB.Driver;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Infrastructure.DataAccess.Configuration;

/// <summary>
/// MongoDB configuration for collections and indexes
/// </summary>
public static class MongoDbConfiguration
{
    /// <summary>
    /// Configures MongoDB collections with indexes and settings
    /// </summary>
    /// <param name="database">The MongoDB database</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task ConfigureAsync(IMongoDatabase database)
    {
        await ConfigureMemoriesCollectionAsync(database);
        await ConfigurePostsCollectionAsync(database);
        await ConfigureCommentsCollectionAsync(database);
        await ConfigureGroupsCollectionAsync(database);
    }

    private static async Task ConfigureMemoriesCollectionAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Memory>("memories");
        
        var indexKeysDefinition = Builders<Memory>.IndexKeys
            .Ascending(x => x.CreatorUserId)
            .Ascending(x => x.CreatedAt);
        
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Memory>(indexKeysDefinition));
        
        // Index for participant searches
        var participantIndexKeys = Builders<Memory>.IndexKeys.Ascending(x => x.ParticipantsIds);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Memory>(participantIndexKeys));
    }

    private static async Task ConfigurePostsCollectionAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Post>("posts");
        
        var indexKeysDefinition = Builders<Post>.IndexKeys
            .Ascending(x => x.MemoryId)
            .Ascending(x => x.CreatedAt);
        
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Post>(indexKeysDefinition));
    }

    private static async Task ConfigureCommentsCollectionAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Comment>("comments");
        
        var indexKeysDefinition = Builders<Comment>.IndexKeys
            .Ascending(x => x.MemoryId)
            .Ascending(x => x.CreatedAt);
        
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(indexKeysDefinition));
        
        // Index for reply posts
        var replyIndexKeys = Builders<Comment>.IndexKeys.Ascending(x => x.ReplyPostId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(replyIndexKeys));
    }

    private static async Task ConfigureGroupsCollectionAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Group>("groups");
        
        // Index for group name searches
        var nameIndexKeys = Builders<Group>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Group>(nameIndexKeys));
    }
}
