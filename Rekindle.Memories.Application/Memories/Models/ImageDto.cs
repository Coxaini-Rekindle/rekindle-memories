namespace Rekindle.Memories.Application.Memories.Models;

public record ImageDto
{
    public Guid FileId { get; init; }
    public List<Guid> ParticipantIds { get; init; } = [];
}