using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Abstractions.Repositories;

public interface IPostRepository
{
    Task InsertPost(Post post, CancellationToken cancellationToken = default, ITransactionContext? transactionContext = null);
    Task<Post?> FindById(Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> FindByMemoryId(Guid memoryId, CancellationToken cancellationToken = default);
    Task UpdatePost(Post post, CancellationToken cancellationToken = default);
    Task DeletePost(Guid postId, CancellationToken cancellationToken = default);
}
