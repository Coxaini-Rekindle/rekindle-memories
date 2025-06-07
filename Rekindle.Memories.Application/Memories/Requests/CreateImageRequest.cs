namespace Rekindle.Memories.Application.Memories.Requests;

public record CreateImageRequest
{
    public Guid? FileId { get; init; } // Optional for existing files
    public Stream? FileStream { get; init; } // For new file uploads
    public string? ContentType { get; init; } // Required for new uploads
    public string? FileName { get; init; } // Optional file name
    public List<Guid> ParticipantIds { get; init; } = []; // Participants in this image
}