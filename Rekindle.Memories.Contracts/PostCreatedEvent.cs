namespace Rekindle.Memories.Contracts;

public class PostCreatedEvent : MemoryEvent
{
    public Guid MemoryId { get; set; }
    public Guid PostId { get; set; }
    public IReadOnlyCollection<Guid> Images { get; set; } = new List<Guid>();
}