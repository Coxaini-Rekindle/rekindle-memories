using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.RemoveReaction;

public class RemovePostReactionCommandHandler : IRequestHandler<RemovePostReactionCommand, ReactionSummaryDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public RemovePostReactionCommandHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<ReactionSummaryDto> Handle(RemovePostReactionCommand request, CancellationToken cancellationToken)
    {
        // Find the post
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

        // Remove the reaction
        post.RemoveReaction(request.UserId);

        // Save the updated post
        await _postRepository.UpdatePost(post, cancellationToken);

        // Return reaction summary
        return post.Reactions.ToReactionSummaryDto(request.UserId);
    }
}