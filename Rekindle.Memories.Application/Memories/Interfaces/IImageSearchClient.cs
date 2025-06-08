using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Interfaces;

public interface IImageSearchClient
{
    Task<IEnumerable<PhotoSearchResult>> SearchImages(
        Guid groupId,
        string query,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default);
}