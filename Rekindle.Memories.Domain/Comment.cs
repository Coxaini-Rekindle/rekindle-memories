using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rekindle.Memories.Domain;

public class Comment : IMemoryBlock
{
    [BsonId, BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatorUserId { get; set; }
    public Guid? ReplyToPostId { get; set; }
    public Guid? ReplyToCommentId { get; set; }
    public List<Reaction> Reactions { get; set; } = [];

    public static Comment Create(Guid memoryId, string content, Guid creatorUserId, Guid? replyToPostId = null, Guid? replyToCommentId = null)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            MemoryId = memoryId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            ReplyToPostId = replyToPostId,
            ReplyToCommentId = replyToCommentId,
            Reactions = []
        };
    }

    public void UpdateContent(string content)
    {
        Content = content;
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

    public bool IsReplyToPost => ReplyToPostId.HasValue;
    public bool IsReplyToComment => ReplyToCommentId.HasValue;
    public bool IsTopLevelComment => !ReplyToPostId.HasValue && !ReplyToCommentId.HasValue;
}