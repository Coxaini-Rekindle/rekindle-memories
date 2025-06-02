using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Queries.GetMemoryActivities;

public class GetMemoryActivitiesQueryHandler : IRequestHandler<GetMemoryActivitiesQuery, CursorPaginationResponse<MemoryActivityDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public GetMemoryActivitiesQueryHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<CursorPaginationResponse<MemoryActivityDto>> Handle(GetMemoryActivitiesQuery request, CancellationToken cancellationToken)
    {
        // Verify memory exists
        var memory = await _memoryRepository.FindById(request.MemoryId, cancellationToken);
        if (memory == null)
        {
            throw new MemoryNotFoundException();
        }

        // Verify user is a member of the group
        var group = await _groupRepository.FindById(memory.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }        // Get posts and comments with extra items to check for more
        var postsTask = _postRepository.FindByMemoryIdWithPagination(
            request.MemoryId, 
            request.PageSize + 1, 
            request.Cursor, 
            cancellationToken);

        var commentsTask = _commentRepository.FindByMemoryIdWithPagination(
            request.MemoryId, 
            request.PageSize + 1, 
            request.Cursor, 
            cancellationToken);

        await Task.WhenAll(postsTask, commentsTask);

        var posts = (await postsTask).ToList();
        var comments = (await commentsTask).ToList();

        // Collect all post and comment IDs that are being replied to
        var replyToPostIds = comments
            .Where(c => c.ReplyToPostId.HasValue)
            .Select(c => c.ReplyToPostId!.Value)
            .Distinct()
            .ToList();

        var replyToCommentIds = comments
            .Where(c => c.ReplyToCommentId.HasValue)
            .Select(c => c.ReplyToCommentId!.Value)
            .Distinct()
            .ToList();

        // Fetch the content of posts and comments being replied to
        var replyToPostsTask = replyToPostIds.Any() 
            ? Task.WhenAll(replyToPostIds.Select(id => _postRepository.FindByIdAsync(id)))
            : Task.FromResult(Array.Empty<Post?>());

        var replyToCommentsTask = replyToCommentIds.Any()
            ? Task.WhenAll(replyToCommentIds.Select(id => _commentRepository.FindByIdAsync(id)))
            : Task.FromResult(Array.Empty<Comment?>());

        await Task.WhenAll(replyToPostsTask, replyToCommentsTask);

        var replyToPosts = (await replyToPostsTask)
            .Where(p => p != null)
            .Cast<Post>()
            .ToDictionary(p => p.Id, p => p.Content);

        var replyToCommentsDict = (await replyToCommentsTask)
            .Where(c => c != null)
            .Cast<Comment>()
            .ToDictionary(c => c.Id, c => c.Content);

        // Convert to activity DTOs and combine
        var postActivities = posts.Select(post => new MemoryActivityDto
        {
            Id = post.Id,
            MemoryId = post.MemoryId,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            CreatorUserId = post.CreatorUserId,
            Type = MemoryActivityType.Post,
            Images = post.Images.Select(img => new ImageDto
            {
                FileId = img.FileId,
                ParticipantIds = img.ParticipantIds
            }).ToList(),
            Reactions = post.Reactions.Select(r => new ReactionDto
            {
                UserId = r.UserId,
                Type = (ReactionTypeDto)r.Type,
                CreatedAt = r.CreatedAt
            }).ToList(),
            ReactionSummary = CreateReactionSummary(post.Reactions, request.UserId),
            ReplyToContent = null // Posts don't reply to anything
        });

        var commentActivities = comments.Select(comment => new MemoryActivityDto
        {
            Id = comment.Id,
            MemoryId = comment.MemoryId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatorUserId = comment.CreatorUserId,
            Type = MemoryActivityType.Comment,
            ReplyToPostId = comment.ReplyToPostId,
            ReplyToCommentId = comment.ReplyToCommentId,
            Reactions = comment.Reactions.Select(r => new ReactionDto
            {
                UserId = r.UserId,
                Type = (ReactionTypeDto)r.Type,
                CreatedAt = r.CreatedAt
            }).ToList(),
            ReactionSummary = CreateReactionSummary(comment.Reactions, request.UserId),
            ReplyToContent = comment.ReplyToPostId.HasValue && replyToPosts.ContainsKey(comment.ReplyToPostId.Value)
                ? replyToPosts[comment.ReplyToPostId.Value]
                : comment.ReplyToCommentId.HasValue && replyToCommentsDict.ContainsKey(comment.ReplyToCommentId.Value)
                    ? replyToCommentsDict[comment.ReplyToCommentId.Value]
                    : null
        });

        // Combine and sort by creation date (newest first)
        var allActivities = postActivities.Concat(commentActivities)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        var hasMore = allActivities.Count > request.PageSize;

        // Remove the extra items if we have more
        if (hasMore)
        {
            allActivities = allActivities.Take(request.PageSize).ToList();
        }

        DateTime? nextCursor = null;
        if (hasMore && allActivities.Any())
        {
            nextCursor = allActivities.Last().CreatedAt;
        }

        return new CursorPaginationResponse<MemoryActivityDto>
        {
            Items = allActivities,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    private static ReactionSummaryDto CreateReactionSummary(List<Reaction> reactions, Guid userId)
    {
        var reactionCounts = reactions
            .GroupBy(r => r.Type)
            .ToDictionary(g => (ReactionTypeDto)g.Key, g => g.Count());

        var userReactions = reactions
            .Where(r => r.UserId == userId)
            .Select(r => (ReactionTypeDto)r.Type)
            .ToList();

        return new ReactionSummaryDto
        {
            TotalCount = reactions.Count,
            ReactionCounts = reactionCounts,
            UserReactions = userReactions
        };
    }
}
