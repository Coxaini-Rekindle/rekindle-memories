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
    public PostDto? MainPost { get; init; }
}