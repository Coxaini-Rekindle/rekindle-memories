using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Abstractions.Repositories;

public interface ICommentRepository
{
    Task InsertComment(Comment comment, CancellationToken cancellationToken = default, ITransactionContext? transactionContext = null);
    Task<Comment?> FindById(Guid commentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindByMemoryId(Guid memoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindByMemoryIdWithPagination(Guid memoryId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindByPostId(Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindByPostIdWithPagination(Guid postId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindRepliesByCommentId(Guid commentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> FindRepliesByCommentIdWithPagination(Guid commentId, int limit, DateTime? cursor = null, CancellationToken cancellationToken = default);
    Task UpdateComment(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteComment(Guid commentId, CancellationToken cancellationToken = default);
    
    // Additional methods for API support
    Task<Comment?> FindByIdAsync(Guid commentId);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(Guid commentId);
    Task DeleteByPostIdAsync(Guid postId);
    Task DeleteRepliesByCommentIdAsync(Guid commentId);
    
    // Pagination method with flexible filters
    Task<CursorPaginationResponse<Comment>> FindWithPaginationAsync(Guid? postId = null, Guid? memoryId = null, int pageSize = 20, DateTime? cursor = null);
}
