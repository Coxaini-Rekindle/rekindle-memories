namespace Rekindle.Memories.Application.Memories.Requests;

public record UpdateCommentRequest
{
    public string Content { get; init; } = string.Empty;
}