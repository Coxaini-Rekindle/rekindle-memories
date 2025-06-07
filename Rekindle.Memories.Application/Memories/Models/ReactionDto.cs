namespace Rekindle.Memories.Application.Memories.Models;

public record ReactionDto
{
    public Guid UserId { get; init; }
    public ReactionTypeDto Type { get; init; }
    public DateTime CreatedAt { get; init; }
}