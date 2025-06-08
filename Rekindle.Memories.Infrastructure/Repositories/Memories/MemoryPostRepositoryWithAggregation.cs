using MongoDB.Driver;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.Repositories.Memories;

public class MemoryPostRepositoryWithAggregation : IMemoryPostRepository
{
    private readonly IMongoCollection<Memory> _memoryCollection;
    private readonly IMongoCollection<Post> _postCollection;

    public MemoryPostRepositoryWithAggregation(IMemoriesDbContext memoriesDbContext)
    {
        _memoryCollection = memoriesDbContext.Memories;
        _postCollection = memoriesDbContext.Posts;
    }

    public async Task<IEnumerable<MemoryWithMainPost>> GetMemoriesWithMainPostsByIds(
        IEnumerable<Guid> memoryIds,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Get filtered memories
        var memoryFilter = Builders<Memory>.Filter.In(m => m.Id, memoryIds);
        var memories = await _memoryCollection
            .Find(memoryFilter)
            .ToListAsync(cancellationToken);

        if (!memories.Any())
            return Enumerable.Empty<MemoryWithMainPost>();

        // Step 2: Get main posts by their IDs
        var mainPostIds = memories.Select(m => m.MainPostId).ToList();
        var postFilter = Builders<Post>.Filter.In(p => p.Id, mainPostIds);
        var posts = await _postCollection
            .Find(postFilter)
            .ToListAsync(cancellationToken);

        // Step 3: Join in memory
        var result = memories
            .Select(memory => new MemoryWithMainPost
            {
                Memory = memory,
                MainPost = posts.FirstOrDefault(p => p.Id == memory.MainPostId)
            })
            .Where(mwp => mwp.MainPost != null) // Only include memories that have a main post
            .ToList();

        return result;
    }
}