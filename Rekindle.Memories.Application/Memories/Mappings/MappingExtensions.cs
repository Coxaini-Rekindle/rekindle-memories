using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Mappings;

/// <summary>
/// Mapping extensions for converting domain entities to DTOs and handling related mappings
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps a Memory domain entity to MemoryDto
    /// </summary>
    /// <param name="memory">The memory domain entity</param>
    /// <param name="mainPost">Optional main post to include in the DTO</param>
    /// <returns>MemoryDto</returns>
    public static MemoryDto ToDto(this Memory memory, Post? mainPost = null)
    {
        return new MemoryDto
        {
            Id = memory.Id,
            GroupId = memory.GroupId,
            Title = memory.Title,
            Description = memory.Description,
            CreatedAt = memory.CreatedAt,
            CreatorUserId = memory.CreatorUserId,
            ParticipantsIds = memory.ParticipantsIds,
            MainPostId = memory.MainPostId,
            MainPost = mainPost?.ToDto()
        };
    }

    /// <summary>
    /// Maps a Post domain entity to PostDto
    /// </summary>
    /// <param name="post">The post domain entity</param>
    /// <param name="currentUserId">Optional current user ID for calculating user-specific reaction summary</param>
    /// <returns>PostDto</returns>
    public static PostDto ToDto(this Post post, Guid? currentUserId = null)
    {
        return new PostDto
        {
            Id = post.Id,
            MemoryId = post.MemoryId,
            Content = post.Content,
            Images = post.Images.Select(img => img.ToDto()).ToList(),
            CreatedAt = post.CreatedAt,
            CreatorUserId = post.CreatorUserId,
            Reactions = post.Reactions.Select(r => r.ToDto()).ToList(),
            ReactionSummary = post.Reactions.ToReactionSummaryDto(currentUserId)
        };
    }

    /// <summary>
    /// Maps a Comment domain entity to CommentDto
    /// </summary>
    /// <param name="comment">The comment domain entity</param>
    /// <param name="currentUserId">Optional current user ID for calculating user-specific reaction summary</param>
    /// <returns>CommentDto</returns>
    public static CommentDto ToDto(this Comment comment, Guid? currentUserId = null)
    {
        return new CommentDto
        {
            Id = comment.Id,
            MemoryId = comment.MemoryId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatorUserId = comment.CreatorUserId,
            ReplyToPostId = comment.ReplyToPostId,
            ReplyToCommentId = comment.ReplyToCommentId,
            Reactions = comment.Reactions.Select(r => r.ToDto()).ToList(),
            ReactionSummary = comment.Reactions.ToReactionSummaryDto(currentUserId)
        };
    }

    /// <summary>
    /// Maps an Image domain entity to ImageDto
    /// </summary>
    /// <param name="image">The image domain entity</param>
    /// <returns>ImageDto</returns>
    public static ImageDto ToDto(this Image image)
    {
        return new ImageDto
        {
            FileId = image.FileId,
            ParticipantIds = image.RecognizedUserIds
        };
    }

    /// <summary>
    /// Maps a Reaction domain entity to ReactionDto
    /// </summary>
    /// <param name="reaction">The reaction domain entity</param>
    /// <returns>ReactionDto</returns>
    public static ReactionDto ToDto(this Reaction reaction)
    {
        return new ReactionDto
        {
            UserId = reaction.UserId,
            Type = reaction.Type.ToDto(),
            CreatedAt = reaction.CreatedAt
        };
    }

    /// <summary>
    /// Maps a Post or Comment to MemoryActivityDto
    /// </summary>
    /// <param name="post">The post domain entity</param>
    /// <param name="currentUserId">Optional current user ID for calculating user-specific reaction summary</param>
    /// <returns>MemoryActivityDto</returns>
    public static MemoryActivityDto ToMemoryActivityDto(this Post post, Guid? currentUserId = null)
    {
        return new MemoryActivityDto
        {
            Id = post.Id,
            MemoryId = post.MemoryId,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            CreatorUserId = post.CreatorUserId,
            Type = MemoryActivityType.Post,
            Reactions = post.Reactions.Select(r => r.ToDto()).ToList(),
            ReactionSummary = post.Reactions.ToReactionSummaryDto(currentUserId),
            Images = post.Images.Select(img => img.ToDto()).ToList(),
            ReplyToPostId = null,
            ReplyToCommentId = null,
            ReplyToContent = null // Posts don't reply to anything
        };
    }

    /// <summary>
    /// Maps a Comment to MemoryActivityDto
    /// </summary>
    /// <param name="comment">The comment domain entity</param>
    /// <param name="currentUserId">Optional current user ID for calculating user-specific reaction summary</param>
    /// <returns>MemoryActivityDto</returns>
    public static MemoryActivityDto ToMemoryActivityDto(this Comment comment, Guid? currentUserId = null)
    {
        return new MemoryActivityDto
        {
            Id = comment.Id,
            MemoryId = comment.MemoryId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatorUserId = comment.CreatorUserId,
            Type = MemoryActivityType.Comment,
            Reactions = comment.Reactions.Select(r => r.ToDto()).ToList(),
            ReactionSummary = comment.Reactions.ToReactionSummaryDto(currentUserId),
            Images = [],
            ReplyToPostId = comment.ReplyToPostId,
            ReplyToCommentId = comment.ReplyToCommentId,
            ReplyToContent = null // Basic mapping doesn't include reply content - use specialized methods when needed
        };
    }

    /// <summary>
    /// Converts a collection of reactions to ReactionSummaryDto
    /// </summary>
    /// <param name="reactions">The collection of reactions</param>
    /// <param name="currentUserId">Optional current user ID for filtering user reactions</param>
    /// <returns>ReactionSummaryDto</returns>
    public static ReactionSummaryDto ToReactionSummaryDto(this IEnumerable<Reaction> reactions,
        Guid? currentUserId = null)
    {
        var reactionList = reactions.ToList();

        return new ReactionSummaryDto
        {
            TotalCount = reactionList.Count,
            ReactionCounts = reactionList
                .GroupBy(r => r.Type.ToDto())
                .ToDictionary(g => g.Key, g => g.Count()),
            UserReactions = currentUserId.HasValue
                ? reactionList
                    .Where(r => r.UserId == currentUserId.Value)
                    .Select(r => r.Type.ToDto())
                    .ToList()
                : []
        };
    }

    /// <summary>
    /// Maps ReactionType enum to ReactionTypeDto enum
    /// </summary>
    /// <param name="reactionType">The domain reaction type</param>
    /// <returns>ReactionTypeDto</returns>
    public static ReactionTypeDto ToDto(this ReactionType reactionType)
    {
        return reactionType switch
        {
            ReactionType.Love => ReactionTypeDto.Love,
            ReactionType.Laugh => ReactionTypeDto.Laugh,
            ReactionType.Wow => ReactionTypeDto.Wow,
            ReactionType.Nostalgic => ReactionTypeDto.Nostalgic,
            ReactionType.Grateful => ReactionTypeDto.Grateful,
            ReactionType.Celebrate => ReactionTypeDto.Celebrate,
            ReactionType.Support => ReactionTypeDto.Support,
            ReactionType.Memories => ReactionTypeDto.Memories,
            ReactionType.Family => ReactionTypeDto.Family,
            ReactionType.Friendship => ReactionTypeDto.Friendship,
            ReactionType.Journey => ReactionTypeDto.Journey,
            ReactionType.Milestone => ReactionTypeDto.Milestone,
            ReactionType.Peaceful => ReactionTypeDto.Peaceful,
            ReactionType.Adventure => ReactionTypeDto.Adventure,
            ReactionType.Warm => ReactionTypeDto.Warm,
            _ => throw new ArgumentOutOfRangeException(nameof(reactionType), reactionType, "Unknown reaction type")
        };
    }

    /// <summary>
    /// Maps ReactionTypeDto enum to ReactionType enum
    /// </summary>
    /// <param name="reactionTypeDto">The DTO reaction type</param>
    /// <returns>ReactionType</returns>
    public static ReactionType ToDomain(this ReactionTypeDto reactionTypeDto)
    {
        return reactionTypeDto switch
        {
            ReactionTypeDto.Love => ReactionType.Love,
            ReactionTypeDto.Laugh => ReactionType.Laugh,
            ReactionTypeDto.Wow => ReactionType.Wow,
            ReactionTypeDto.Nostalgic => ReactionType.Nostalgic,
            ReactionTypeDto.Grateful => ReactionType.Grateful,
            ReactionTypeDto.Celebrate => ReactionType.Celebrate,
            ReactionTypeDto.Support => ReactionType.Support,
            ReactionTypeDto.Memories => ReactionType.Memories,
            ReactionTypeDto.Family => ReactionType.Family,
            ReactionTypeDto.Friendship => ReactionType.Friendship,
            ReactionTypeDto.Journey => ReactionType.Journey,
            ReactionTypeDto.Milestone => ReactionType.Milestone,
            ReactionTypeDto.Peaceful => ReactionType.Peaceful,
            ReactionTypeDto.Adventure => ReactionType.Adventure,
            ReactionTypeDto.Warm => ReactionType.Warm,
            _ => throw new ArgumentOutOfRangeException(nameof(reactionTypeDto), reactionTypeDto,
                "Unknown reaction type DTO")
        };
    }
}