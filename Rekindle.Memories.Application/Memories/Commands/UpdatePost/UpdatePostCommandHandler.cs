using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new PostNotFoundException();
        }

        // Verify the user owns the post
        if (post.CreatorUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only update your own posts");
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

        // Update the post
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            post.UpdateContent(request.Content);
        }

        await _postRepository.UpdatePost(post, cancellationToken);

        return post.ToDto(request.UserId);
    }
}