namespace Rekindle.Memories.Application.Memories.Requests;

public record CreateCommentRequest
{
    public string Content { get; init; } = string.Empty;
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }
}