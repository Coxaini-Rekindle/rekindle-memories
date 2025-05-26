namespace Rekindle.Memories.Domain;

public class Post : IMemoryBlock
{
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<Image> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class Image
{
    public Guid FileId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = [];
}