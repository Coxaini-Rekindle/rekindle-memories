namespace Rekindle.Memories.Application.Memories.Models;

public record MemoryActivityDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public MemoryActivityType Type { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();

    // Post-specific properties
    public List<ImageDto> Images { get; init; } = [];

    // Comment-specific properties
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }

    // Reply content properties
    public string? ReplyToContent { get; init; } // Content of the post or comment being replied to

    // Helper properties for comments
    public bool IsReplyToPost => ReplyToPostId.HasValue;
    public bool IsReplyToComment => ReplyToCommentId.HasValue;

    public bool IsTopLevelComment =>
        Type == MemoryActivityType.Comment && !ReplyToPostId.HasValue && !ReplyToCommentId.HasValue;
}