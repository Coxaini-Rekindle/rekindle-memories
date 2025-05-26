namespace Rekindle.Memories.Domain;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<User> Members { get; set; } = new();
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Guid? AvatarFileId { get; set; }
}

public class ImageParticipant
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public Guid? AvatarFileId { get; set; }
}