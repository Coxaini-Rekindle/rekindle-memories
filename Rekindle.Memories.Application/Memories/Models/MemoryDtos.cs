namespace Rekindle.Memories.Application.Memories.Models;

public record MemoryDto
{
    public Guid Id { get; init; }
    public Guid GroupId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public List<Guid> ParticipantsIds { get; init; } = [];
    public Guid MainPostId { get; init; }
}

public record PostDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<ImageDto> Images { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
}

public record ImageDto
{
    public Guid FileId { get; init; }
    public List<Guid> ParticipantIds { get; init; } = [];
}

public record CreateMemoryRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
}

public record CreateImageRequest
{
    public Guid? FileId { get; init; } // Optional for existing files
    public Stream? FileStream { get; init; } // For new file uploads
    public string? ContentType { get; init; } // Required for new uploads
    public string? FileName { get; init; } // Optional file name
}
