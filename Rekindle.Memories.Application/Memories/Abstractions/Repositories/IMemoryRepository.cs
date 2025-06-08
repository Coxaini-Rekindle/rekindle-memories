using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Abstractions.Repositories;

public interface IMemoryRepository
{
    Task InsertMemory(Memory memory, CancellationToken cancellationToken = default,
        ITransactionContext? transactionContext = null);

    Task<Memory?> FindById(Guid memoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Memory>> FindByIds(IEnumerable<Guid> memoryIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<Memory>> FindByGroupId(Guid groupId, int limit, DateTime? cursor = null,
        CancellationToken cancellationToken = default);

    Task UpdateMemory(Memory memory, CancellationToken cancellationToken = default);
    Task DeleteMemory(Guid memoryId, CancellationToken cancellationToken = default);
}