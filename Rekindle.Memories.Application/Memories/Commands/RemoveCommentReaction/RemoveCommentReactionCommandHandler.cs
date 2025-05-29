using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.RemoveCommentReaction;

public class RemoveCommentReactionCommandHandler : IRequestHandler<RemoveCommentReactionCommand, ReactionSummaryDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public RemoveCommentReactionCommandHandler(
        ICommentRepository commentRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _commentRepository = commentRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<ReactionSummaryDto> Handle(RemoveCommentReactionCommand request, CancellationToken cancellationToken)
    {
        // Find the comment
        var comment = await _commentRepository.FindById(request.CommentId, cancellationToken);
        if (comment == null)
        {
            throw new CommentNotFoundException();
        }

        // Verify memory exists and user is a member of the group
        var memory = await _memoryRepository.FindById(comment.MemoryId, cancellationToken);
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

        // Remove the reaction
        comment.RemoveReaction(request.UserId);

        // Save the updated comment
        await _commentRepository.UpdateComment(comment, cancellationToken);

        // Return reaction summary
        return comment.Reactions.ToReactionSummaryDto(request.UserId);
    }
}
