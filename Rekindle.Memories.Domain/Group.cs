using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rekindle.Memories.Domain;

public class Group
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<User> Members { get; set; } = new();
    
    public static Group Create(Guid id,string name, string description, User creator)
    {
        return new Group
        {
            Id = id,
            Name = name,
            Description = description,
            Members = [creator]
        };
    }

    public void UpdateDetails(string messageName, string messageDescription)
    {
        Name = messageName;
        Description = messageDescription;
    }

    public User AddMember(Guid userId, string mame, string userName, Guid? avatarFileId)
    {
        var newMember = User.Create(userId, mame, userName, avatarFileId);
        Members.Add(newMember);
        return newMember;
    }

    public void RemoveMember(Guid userId)
    {
        var member = Members.FirstOrDefault(m => m.Id == userId);
        if (member != null)
        {
            Members.Remove(member);
        }
    }

    public void UpdateUserAvatar(Guid userId, Guid? newAvatarFileId)
    {
        var member = Members.FirstOrDefault(m => m.Id == userId);
        if (member != null)
        {
            member.AvatarFileId = newAvatarFileId;
        }
    }

    public void UpdateUserName(Guid userId, string newName)
    {
        var member = Members.FirstOrDefault(m => m.Id == userId);
        if (member != null)
        {
            member.Name = newName;
        }
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Guid? AvatarFileId { get; set; }
    
    public static User Create(Guid id, string name, string username, Guid? avatarFileId = null)
    {
        return new User
        {
            Id = id,
            Name = name,
            Username = username,
            AvatarFileId = avatarFileId
        };
    }
}

public class ImageParticipant
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public Guid? AvatarFileId { get; set; }
}