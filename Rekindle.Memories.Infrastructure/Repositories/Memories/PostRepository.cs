using MongoDB.Driver;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.Repositories.Memories;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _postCollection;

    public PostRepository(IMemoriesDbContext memoriesDbContext)
    {
        _postCollection = memoriesDbContext.Posts;
    }

    public async Task InsertPost(Post post, CancellationToken cancellationToken = default, ITransactionContext? transactionContext = null)
    {
        var options = new InsertOneOptions();
        var session = (transactionContext as MongoTransactionContext)?.Session;
        
        if (session != null)
        {
            await _postCollection.InsertOneAsync(session, post, options, cancellationToken);
        }
        else
        {
            await _postCollection.InsertOneAsync(post, options, cancellationToken);
        }
    }

    public async Task<Post?> FindById(Guid postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
        return await _postCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> FindByMemoryId(Guid memoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.MemoryId, memoryId);
        return await _postCollection
            .Find(filter)
            .SortBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> FindByMemoryIdWithPagination(Guid memoryId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Post>.Filter;
        var filter = filterBuilder.Eq(p => p.MemoryId, memoryId);

        if (cursor.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lt(p => p.CreatedAt, cursor.Value));
        }

        return await _postCollection
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePost(Post post, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
        await _postCollection.ReplaceOneAsync(filter, post, new ReplaceOptions(), cancellationToken);
    }    public async Task DeletePost(Guid postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
        await _postCollection.DeleteOneAsync(filter, cancellationToken);
    }

    // Additional API support methods
    public async Task<Post?> FindByIdAsync(Guid postId)
    {
        return await FindById(postId);
    }

    public async Task UpdateAsync(Post post)
    {
        await UpdatePost(post);
    }

    public async Task DeleteAsync(Guid postId)
    {
        await DeletePost(postId);
    }

    public async Task<CursorPaginationResponse<Post>> FindByMemoryIdWithPaginationAsync(Guid memoryId, int pageSize = 20, DateTime? cursor = null)
    {
        var posts = await FindByMemoryIdWithPagination(memoryId, pageSize + 1, cursor);
        var postList = posts.ToList();
        var hasMore = postList.Count > pageSize;
        
        if (hasMore)
        {
            postList = postList.Take(pageSize).ToList();
        }

        DateTime? nextCursor = null;
        if (hasMore && postList.Any())
        {
            nextCursor = postList.Last().CreatedAt;
        }

        return new CursorPaginationResponse<Post>
        {
            Items = postList,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
