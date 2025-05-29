using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rekindle.Memories.Domain;

public class Memory
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatorUserId { get; set; }
    public List<Guid> ParticipantsIds { get; set; } = [];
    public Guid MainPostId { get; set; }
      public static Memory Create(Guid groupId, string title, string description, Guid creatorUserId, Guid mainPostId)
    {
        return new Memory
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            ParticipantsIds = [], // Empty - will be populated by face recognition service
            MainPostId = mainPostId
        };
    }    public void UpdateDetails(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public void SetMainPost(Guid mainPostId)
    {
        MainPostId = mainPostId;
    }

    public void AddParticipant(Guid participantId)
    {
        if (!ParticipantsIds.Contains(participantId))
        {
            ParticipantsIds.Add(participantId);
        }
    }

    public void RemoveParticipant(Guid participantId)
    {
        ParticipantsIds.Remove(participantId);
    }

    public void SetParticipants(List<Guid> participantIds)
    {
        ParticipantsIds = participantIds ?? [];
    }
}