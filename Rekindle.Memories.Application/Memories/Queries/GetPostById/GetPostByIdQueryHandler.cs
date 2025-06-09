using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Queries.GetPostById;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new PostNotFoundException();
        }

        // Verify memory exists and user is a member of the group
        var memory = await _memoryRepository.FindById(post.MemoryId, cancellationToken);
        if (memory == null)
        {
            throw new MemoryNotFoundException();
        }

        var group = await _groupRepository.FindByIdAsync(memory.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }

        return new PostDto
        {
            Id = post.Id,
            MemoryId = post.MemoryId,
            Content = post.Content,
            Images = post.Images.Select(img => new ImageDto
            {
                FileId = img.FileId,
                ParticipantIds = img.RecognizedUserIds
            }).ToList(),
            CreatedAt = post.CreatedAt,
            CreatorUserId = post.CreatorUserId,
            Reactions = post.Reactions.Select(r => new ReactionDto
            {
                UserId = r.UserId,
                Type = (ReactionTypeDto)r.Type,
                CreatedAt = r.CreatedAt
            }).ToList(),
            ReactionSummary = CreateReactionSummary(post.Reactions, request.UserId)
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