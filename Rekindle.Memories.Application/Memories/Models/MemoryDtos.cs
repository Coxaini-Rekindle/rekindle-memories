namespace Rekindle.Memories.Application.Memories.Models;

public record MemoryDto
{
    public Guid Id { get; init; }
    public Guid GroupId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public List<Guid> ParticipantsIds { get; init; } = [];
    public Guid MainPostId { get; init; }
    public PostDto? MainPost { get; init; }
}

public record PostDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<ImageDto> Images { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();
}

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();
    public bool IsReplyToPost => ReplyToPostId.HasValue;
    public bool IsReplyToComment => ReplyToCommentId.HasValue;
    public bool IsTopLevelComment => !ReplyToPostId.HasValue && !ReplyToCommentId.HasValue;
}

public record ImageDto
{
    public Guid FileId { get; init; }
    public List<Guid> ParticipantIds { get; init; } = [];
}

public record ReactionDto
{
    public Guid UserId { get; init; }
    public ReactionTypeDto Type { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ReactionSummaryDto
{
    public int TotalCount { get; init; }
    public Dictionary<ReactionTypeDto, int> ReactionCounts { get; init; } = new();
    public List<ReactionTypeDto> UserReactions { get; init; } = []; // Current user's reactions
}

public enum ReactionTypeDto
{
    Love,           // ❤️ Classic love/heart
    Laugh,          // 😂 Funny/laughing
    Wow,            // 😮 Amazing/surprised
    Nostalgic,      // 🥺 Missing those times/nostalgic
    Grateful,       // 🙏 Thankful/grateful
    Celebrate,      // 🎉 Party/celebration
    Support,        // 💪 Supportive/strong
    Memories,       // 📸 Memory lane/camera
    Family,         // 👨‍👩‍👧‍👦 Family vibes
    Friendship,     // 🤝 Friendship/bond
    Journey,        // 🛤️ Life journey/path
    Milestone,      // 🏆 Achievement/milestone
    Peaceful,       // 🕊️ Peaceful/serene
    Adventure,      // 🌟 Adventure/exciting
    Warm           // ☀️ Warm feelings/sunshine
}

public record CreateMemoryRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
}

public record CreatePostRequest
{
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
}

public record CreateCommentRequest
{
    public string Content { get; init; } = string.Empty;
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }
}

public record UpdatePostRequest
{
    public string Content { get; init; } = string.Empty;
}

public record UpdateCommentRequest
{
    public string Content { get; init; } = string.Empty;
}

public record AddReactionRequest
{
    public ReactionTypeDto Type { get; init; }
}

public record CreateImageRequest
{
    public Guid? FileId { get; init; } // Optional for existing files
    public Stream? FileStream { get; init; } // For new file uploads
    public string? ContentType { get; init; } // Required for new uploads
    public string? FileName { get; init; } // Optional file name
    public List<Guid> ParticipantIds { get; init; } = []; // Participants in this image
}

public record MemoryActivityDto
{
    public Guid Id { get; init; }
    public Guid MemoryId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatorUserId { get; init; }
    public MemoryActivityType Type { get; init; }
    public List<ReactionDto> Reactions { get; init; } = [];
    public ReactionSummaryDto ReactionSummary { get; init; } = new();
    
    // Post-specific properties
    public List<ImageDto> Images { get; init; } = [];
    
    // Comment-specific properties
    public Guid? ReplyToPostId { get; init; }
    public Guid? ReplyToCommentId { get; init; }
    
    // Helper properties for comments
    public bool IsReplyToPost => ReplyToPostId.HasValue;
    public bool IsReplyToComment => ReplyToCommentId.HasValue;
    public bool IsTopLevelComment => Type == MemoryActivityType.Comment && !ReplyToPostId.HasValue && !ReplyToCommentId.HasValue;
}

public enum MemoryActivityType
{
    Post,
    Comment
}
