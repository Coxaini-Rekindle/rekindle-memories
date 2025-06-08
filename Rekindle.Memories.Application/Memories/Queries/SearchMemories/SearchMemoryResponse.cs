namespace Rekindle.Memories.Application.Memories.Queries.SearchMemories;

public record SearchMemoryResponse(
    Guid MemoryId,
    DateTime CreatedAt,
    MemoryMatchedPhoto Photo,
    string Title,
    string Content
);

public record MemoryMatchedPhoto(
    Guid PhotoId,
    Guid PostId,
    string Title,
    string Content,
    Guid CreatorUserId
);