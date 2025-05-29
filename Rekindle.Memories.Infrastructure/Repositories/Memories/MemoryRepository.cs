using MongoDB.Driver;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Domain;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.Repositories.Memories;

public class MemoryRepository : IMemoryRepository
{
    private readonly IMongoCollection<Memory> _memoryCollection;

    public MemoryRepository(IMemoriesDbContext memoriesDbContext)
    {
        _memoryCollection = memoriesDbContext.Memories;
    }

    public async Task InsertMemory(Memory memory, CancellationToken cancellationToken = default, ITransactionContext? transactionContext = null)
    {
        var options = new InsertOneOptions();
        var session = (transactionContext as MongoTransactionContext)?.Session;
        
        if (session != null)
        {
            await _memoryCollection.InsertOneAsync(session, memory, options, cancellationToken);
        }
        else
        {
            await _memoryCollection.InsertOneAsync(memory, options, cancellationToken);
        }
    }

    public async Task<Memory?> FindById(Guid memoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Memory>.Filter.Eq(m => m.Id, memoryId);
        return await _memoryCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Memory>> FindByGroupId(Guid groupId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Memory>.Filter;
        var filter = filterBuilder.Eq(m => m.GroupId, groupId);

        if (cursor.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lt(m => m.CreatedAt, cursor.Value));
        }

        return await _memoryCollection
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateMemory(Memory memory, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Memory>.Filter.Eq(m => m.Id, memory.Id);
        await _memoryCollection.ReplaceOneAsync(filter, memory, new ReplaceOptions(), cancellationToken);
    }

    public async Task DeleteMemory(Guid memoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Memory>.Filter.Eq(m => m.Id, memoryId);
        await _memoryCollection.DeleteOneAsync(filter, cancellationToken);
    }
}
