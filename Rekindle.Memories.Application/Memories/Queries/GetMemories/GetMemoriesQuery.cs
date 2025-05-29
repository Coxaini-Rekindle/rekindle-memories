using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetMemories;

public record GetMemoriesQuery : IRequest<CursorPaginationResponse<MemoryDto>>
{
    public Guid GroupId { get; init; }
    public int Limit { get; init; } = 20;
    public DateTime? Cursor { get; init; }
    public Guid UserId { get; init; }
}

public class GetMemoriesQueryHandler : IRequestHandler<GetMemoriesQuery, CursorPaginationResponse<MemoryDto>>
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IPostRepository _postRepository;

    public GetMemoriesQueryHandler(
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository,
        IPostRepository postRepository)
    {
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
        _postRepository = postRepository;
    }

    public async Task<CursorPaginationResponse<MemoryDto>> Handle(GetMemoriesQuery request, CancellationToken cancellationToken)
    {
        // Validate that group exists and user is a member
        var group = await _groupRepository.FindById(request.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }

        // Get memories with one extra to check if there are more
        var memories = await _memoryRepository.FindByGroupId(
            request.GroupId, 
            request.Limit + 1, 
            request.Cursor, 
            cancellationToken);

        var memoryList = memories.ToList();
        var hasMore = memoryList.Count > request.Limit;
        
        // Remove the extra item if we have more
        if (hasMore)
        {
            memoryList = memoryList.Take(request.Limit).ToList();
        }        DateTime? nextCursor = null;
        if (hasMore && memoryList.Any())
        {
            nextCursor = memoryList.Last().CreatedAt;
        }

        // Fetch main posts for all memories
        var mainPostIds = memoryList.Select(m => m.MainPostId).ToList();
        var mainPostTasks = mainPostIds.Select(postId => _postRepository.FindById(postId, cancellationToken));
        var mainPosts = await Task.WhenAll(mainPostTasks);
        
        // Create a dictionary for quick lookup
        var mainPostsDict = mainPosts
            .Where(p => p != null)
            .ToDictionary(p => p!.Id, p => p!);

        var memoryDtos = memoryList.Select(m => new MemoryDto
        {
            Id = m.Id,
            GroupId = m.GroupId,
            Title = m.Title,
            Description = m.Description,
            CreatedAt = m.CreatedAt,
            CreatorUserId = m.CreatorUserId,
            ParticipantsIds = m.ParticipantsIds,
            MainPostId = m.MainPostId,
            MainPost = mainPostsDict.TryGetValue(m.MainPostId, out var mainPost) ? new PostDto
            {
                Id = mainPost.Id,
                MemoryId = mainPost.MemoryId,
                Content = mainPost.Content,
                Images = mainPost.Images.Select(img => new ImageDto
                {
                    FileId = img.FileId,
                    ParticipantIds = img.ParticipantIds
                }).ToList(),
                CreatedAt = mainPost.CreatedAt,
                CreatorUserId = mainPost.CreatorUserId
            } : null
        });

        return new CursorPaginationResponse<MemoryDto>
        {
            Items = memoryDtos,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
