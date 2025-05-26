namespace Rekindle.Memories.Domain;

public class Comment : IMemoryBlock
{
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatorUserId { get; set; }
    public Guid? ReplyPostId { get; set; }
}