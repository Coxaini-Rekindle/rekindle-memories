namespace Rekindle.Memories.Application.Memories.Models;

public record PhotoSearchResult(
    Guid FileId,
    Guid MemoryId,
    Guid PostId
);