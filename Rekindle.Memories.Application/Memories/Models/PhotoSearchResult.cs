namespace Rekindle.Memories.Application.Memories.Models;

public record PhotoSearchResult(
    Guid MemoryId,
    Guid PhotoId,
    Guid PostId,
    Guid PublisherUserId,
    DateTime CreatedAt,
    string Title,
    string Content
);