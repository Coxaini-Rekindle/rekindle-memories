namespace Rekindle.Memories.Application.Memories.Models;

public record PostDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<ImageDto> Images { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();
}