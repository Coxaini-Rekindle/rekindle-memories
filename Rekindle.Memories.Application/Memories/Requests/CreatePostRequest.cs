namespace Rekindle.Memories.Application.Memories.Requests;

public record CreatePostRequest
{
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
}