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
    public bool IsMainPost { get; set; } = false;
    public List<Reaction> Reactions { get; set; } = [];

    public static Post Create(Guid memoryId, string content, List<Image> images, Guid creatorUserId,
        bool isMainPost = false)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            MemoryId = memoryId,
            Content = content,
            Images = images ?? [],
            CreatedAt = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            IsMainPost = isMainPost,
            Reactions = []
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

    public void AddReaction(Reaction reaction)
    {
        // Remove existing reaction from same user if any
        Reactions.RemoveAll(r => r.UserId == reaction.UserId);
        Reactions.Add(reaction);
    }

    public void RemoveReaction(Guid userId)
    {
        Reactions.RemoveAll(r => r.UserId == userId);
    }

    public void UpdateReaction(Guid userId, ReactionType newReactionType)
    {
        var existingReaction = Reactions.FirstOrDefault(r => r.UserId == userId);
        if (existingReaction != null)
        {
            existingReaction.Type = newReactionType;
            existingReaction.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            AddReaction(Reaction.Create(userId, newReactionType));
        }
    }
}

public class Image
{
    public Guid FileId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = [];
}

public class Reaction
{
    public Guid UserId { get; set; }
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public static Reaction Create(Guid userId, ReactionType type)
    {
        return new Reaction
        {
            UserId = userId,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum ReactionType
{
    Love, // ❤️ Classic love/heart
    Laugh, // 😂 Funny/laughing
    Wow, // 😮 Amazing/surprised
    Nostalgic, // 🥺 Missing those times/nostalgic
    Grateful, // 🙏 Thankful/grateful
    Celebrate, // 🎉 Party/celebration
    Support, // 💪 Supportive/strong
    Memories, // 📸 Memory lane/camera
    Family, // 👨‍👩‍👧‍👦 Family vibes
    Friendship, // 🤝 Friendship/bond
    Journey, // 🛤️ Life journey/path
    Milestone, // 🏆 Achievement/milestone
    Peaceful, // 🕊️ Peaceful/serene
    Adventure, // 🌟 Adventure/exciting
    Warm // ☀️ Warm feelings/sunshine
}