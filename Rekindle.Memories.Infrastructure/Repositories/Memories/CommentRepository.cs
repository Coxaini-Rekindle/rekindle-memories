using MongoDB.Driver;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.Repositories.Memories;

public class CommentRepository : ICommentRepository
{
    private readonly IMongoCollection<Comment> _commentCollection;

    public CommentRepository(IMemoriesDbContext memoriesDbContext)
    {
        _commentCollection = memoriesDbContext.Comments;
    }

    public async Task InsertComment(Comment comment, CancellationToken cancellationToken = default, ITransactionContext? transactionContext = null)
    {
        var options = new InsertOneOptions();
        var session = (transactionContext as MongoTransactionContext)?.Session;
        
        if (session != null)
        {
            await _commentCollection.InsertOneAsync(session, comment, options, cancellationToken);
        }
        else
        {
            await _commentCollection.InsertOneAsync(comment, options, cancellationToken);
        }
    }

    public async Task<Comment?> FindById(Guid commentId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
        return await _commentCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindByMemoryId(Guid memoryId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.MemoryId, memoryId);
        return await _commentCollection
            .Find(filter)
            .SortBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindByMemoryIdWithPagination(Guid memoryId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Comment>.Filter;
        var filter = filterBuilder.Eq(c => c.MemoryId, memoryId);

        if (cursor.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lt(c => c.CreatedAt, cursor.Value));
        }

        return await _commentCollection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindByPostId(Guid postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.ReplyToPostId, postId);
        return await _commentCollection
            .Find(filter)
            .SortBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindByPostIdWithPagination(Guid postId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Comment>.Filter;
        var filter = filterBuilder.Eq(c => c.ReplyToPostId, postId);

        if (cursor.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lt(c => c.CreatedAt, cursor.Value));
        }

        return await _commentCollection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindRepliesByCommentId(Guid commentId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.ReplyToCommentId, commentId);
        return await _commentCollection
            .Find(filter)
            .SortBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindRepliesByCommentIdWithPagination(Guid commentId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Comment>.Filter;
        var filter = filterBuilder.Eq(c => c.ReplyToCommentId, commentId);

        if (cursor.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lt(c => c.CreatedAt, cursor.Value));
        }

        return await _commentCollection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateComment(Comment comment, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, comment.Id);
        await _commentCollection.ReplaceOneAsync(filter, comment, new ReplaceOptions(), cancellationToken);
    }    public async Task DeleteComment(Guid commentId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
        await _commentCollection.DeleteOneAsync(filter, cancellationToken);
    }

    // Additional API support methods
    public async Task<Comment?> FindByIdAsync(Guid commentId)
    {
        return await FindById(commentId);
    }

    public async Task UpdateAsync(Comment comment)
    {
        await UpdateComment(comment);
    }

    public async Task DeleteAsync(Guid commentId)
    {
        await DeleteComment(commentId);
    }

    public async Task DeleteByPostIdAsync(Guid postId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.ReplyToPostId, postId);
        await _commentCollection.DeleteManyAsync(filter);
    }

    public async Task DeleteRepliesByCommentIdAsync(Guid commentId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.ReplyToCommentId, commentId);
        await _commentCollection.DeleteManyAsync(filter);
    }

    public async Task<CursorPaginationResponse<Comment>> FindWithPaginationAsync(Guid? postId = null, Guid? memoryId = null, int pageSize = 20, DateTime? cursor = null)
    {
        var filterBuilder = Builders<Comment>.Filter;
        var filters = new List<FilterDefinition<Comment>>();

        if (postId.HasValue)
        {
            filters.Add(filterBuilder.Eq(c => c.ReplyToPostId, postId.Value));
        }

        if (memoryId.HasValue)
        {
            filters.Add(filterBuilder.Eq(c => c.MemoryId, memoryId.Value));
        }

        if (cursor.HasValue)
        {
            filters.Add(filterBuilder.Lt(c => c.CreatedAt, cursor.Value));
        }

        var filter = filters.Any() 
            ? filterBuilder.And(filters) 
            : filterBuilder.Empty;

        var comments = await _commentCollection
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Limit(pageSize + 1)
            .ToListAsync();

        var hasMore = comments.Count > pageSize;
        
        if (hasMore)
        {
            comments = comments.Take(pageSize).ToList();
        }

        DateTime? nextCursor = null;
        if (hasMore && comments.Any())
        {
            nextCursor = comments.Last().CreatedAt;
        }

        return new CursorPaginationResponse<Comment>
        {
            Items = comments,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
