using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.AddCommentReaction;

public class AddCommentReactionCommandHandler : IRequestHandler<AddCommentReactionCommand, ReactionSummaryDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public AddCommentReactionCommandHandler(
        ICommentRepository commentRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _commentRepository = commentRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<ReactionSummaryDto> Handle(AddCommentReactionCommand request, CancellationToken cancellationToken)
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

        var group = await _groupRepository.FindByIdAsync(memory.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        } // Add or update the reaction

        var reactionType = request.ReactionType.ToDomain();
        comment.UpdateReaction(request.UserId, reactionType);

        // Save the updated comment
        await _commentRepository.UpdateComment(comment, cancellationToken);

        // Return reaction summary
        return comment.Reactions.ToReactionSummaryDto(request.UserId);
    }
}