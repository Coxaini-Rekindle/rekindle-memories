using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.AddReaction;

public class AddPostReactionCommandHandler : IRequestHandler<AddPostReactionCommand, ReactionSummaryDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public AddPostReactionCommandHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<ReactionSummaryDto> Handle(AddPostReactionCommand request, CancellationToken cancellationToken)
    {
        // Find the post
        var post = await _postRepository.FindById(request.PostId, cancellationToken);
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

        var group = await _groupRepository.FindById(memory.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }

        // Add or update the reaction
        var reactionType = request.ReactionType.ToDomain();
        post.UpdateReaction(request.UserId, reactionType);

        // Save the updated post
        await _postRepository.UpdatePost(post, cancellationToken);

        // Return reaction summary
        return post.Reactions.ToReactionSummaryDto(request.UserId);
    }
}
