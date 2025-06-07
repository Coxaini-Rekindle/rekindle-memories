namespace Rekindle.Memories.Application.Memories.Models;

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();
    public bool IsReplyToPost => ReplyToPostId.HasValue;
    public bool IsReplyToComment => ReplyToCommentId.HasValue;
    public bool IsTopLevelComment => !ReplyToPostId.HasValue && !ReplyToCommentId.HasValue;
}