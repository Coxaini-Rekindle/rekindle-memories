namespace Rekindle.Memories.Application.Memories.Requests;

public record CreateMemoryRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
}