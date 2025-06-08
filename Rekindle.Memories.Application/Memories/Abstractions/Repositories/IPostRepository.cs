using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Abstractions.Repositories;

public interface IPostRepository
{
    Task InsertPost(Post post, CancellationToken cancellationToken = default,
        ITransactionContext? transactionContext = null);

    Task<Post?> FindById(Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> FindByMemoryId(Guid memoryId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Post>> FindByMemoryIdWithPagination(Guid memoryId, int limit, DateTime? cursor = null,
        CancellationToken cancellationToken = default);

    Task UpdatePost(Post post, CancellationToken cancellationToken = default);
    Task DeletePost(Guid postId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Post>> GetMainPostsByMemoryIds(IEnumerable<Guid> memoryIds,
        CancellationToken cancellationToken = default);

    // Additional methods for API support
    Task<Post?> FindByIdAsync(Guid postId);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid postId);

    // Pagination method with cursor support
    Task<CursorPaginationResponse<Post>> FindByMemoryIdWithPaginationAsync(Guid memoryId, int pageSize = 20,
        DateTime? cursor = null);
}