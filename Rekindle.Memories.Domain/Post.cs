using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rekindle.Memories.Domain;

public class Post : IMemoryBlock
{
    [BsonId, BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public string Content { get; set; } = null!;
    public List<Image> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public Guid CreatorUserId { get; set; }
      public static Post Create(Guid memoryId, string content, List<Image> images, Guid creatorUserId)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            MemoryId = memoryId,
            Content = content,
            Images = images ?? [],
            CreatedAt = DateTime.UtcNow,
            CreatorUserId = creatorUserId
        };
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }

    public void AddImage(Image image)
    {
        Images.Add(image);
    }

    public void RemoveImage(Guid fileId)
    {
        Images.RemoveAll(img => img.FileId == fileId);
    }
}

public class Image
{
    public Guid FileId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = [];
}