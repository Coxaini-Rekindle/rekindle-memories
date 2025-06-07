namespace Rekindle.Memories.Application.Memories.Requests;

public record UpdatePostRequest
{
    public string Content { get; init; } = string.Empty;
}