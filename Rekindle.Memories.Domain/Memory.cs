namespace Rekindle.Memories.Domain;

public class Memory
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatorUserId { get; set; }
    public List<Guid> ParticipantsIds { get; set; } = [];
}