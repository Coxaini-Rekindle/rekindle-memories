using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Interfaces;

public interface IImageSearchClient
{
    Task<IEnumerable<PhotoSearchResult>> SearchImagesAsync(
        Guid groupId,
        string query,
        IEnumerable<Guid> participants,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default);

    Task MergeUsersAsync(
        Guid groupId,
        Guid userId,
        Guid targetUserId,
        CancellationToken cancellationToken = default);
}