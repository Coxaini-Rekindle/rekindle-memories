using MongoDB.Driver;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Infrastructure.DataAccess;

/// <summary>
/// Database context interface for Memories domain
/// </summary>
public interface IMemoriesDbContext
{
    /// <summary>
    /// Gets the MongoDB client instance for transactions
    /// </summary>
    IMongoClient Client { get; }
    
    /// <summary>
    /// Gets the MongoDB database instance
    /// </summary>
    IMongoDatabase Database { get; }
    
    /// <summary>
    /// Gets the Memories collection
    /// </summary>
    IMongoCollection<Memory> Memories { get; }
    
    /// <summary>
    /// Gets the Posts collection
    /// </summary>
    IMongoCollection<Post> Posts { get; }
    
    /// <summary>
    /// Gets the Comments collection
    /// </summary>
    IMongoCollection<Comment> Comments { get; }
    
    /// <summary>
    /// Gets the Groups collection
    /// </summary>
    IMongoCollection<Group> Groups { get; }
    
    /// <summary>
    /// Gets a collection for a specific type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>The MongoDB collection</returns>
    IMongoCollection<T> GetCollection<T>(string? name = null);
}
