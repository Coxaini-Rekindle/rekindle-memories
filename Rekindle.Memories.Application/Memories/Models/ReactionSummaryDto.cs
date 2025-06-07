namespace Rekindle.Memories.Application.Memories.Models;

public record ReactionSummaryDto
{
    public int TotalCount { get; init; }
    public Dictionary<ReactionTypeDto, int> ReactionCounts { get; init; } = new();
    public List<ReactionTypeDto> UserReactions { get; init; } = []; // Current user's reactions
}