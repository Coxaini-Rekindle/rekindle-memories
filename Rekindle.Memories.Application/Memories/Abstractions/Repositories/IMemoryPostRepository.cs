using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Abstractions.Repositories;

public interface IMemoryPostRepository
{
    Task<IEnumerable<MemoryWithMainPost>> GetMemoriesWithMainPostsByIds(
        IEnumerable<Guid> memoryIds, 
        CancellationToken cancellationToken = default);
}