using MongoDB.Driver;

namespace Rekindle.Memories.Infrastructure.DataAccess;

public interface ICollectionFactory
{
    IMongoCollection<T> GetCollection<T>();
}