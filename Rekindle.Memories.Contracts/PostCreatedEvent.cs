namespace Rekindle.Memories.Contracts;

public class PostCreatedEvent : MemoryEvent
{
    public Guid MemoryId { get; set; }
    public Guid GroupId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public IReadOnlyCollection<Guid> Images { get; set; } = new List<Guid>();
}