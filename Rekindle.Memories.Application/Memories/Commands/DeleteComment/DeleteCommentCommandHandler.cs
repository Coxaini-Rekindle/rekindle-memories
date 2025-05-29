using MediatR;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;

namespace Rekindle.Memories.Application.Memories.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.FindByIdAsync(request.CommentId);
        if (comment == null)
            throw new CommentNotFoundException();

        // Verify the user owns the comment
        if (comment.CreatorUserId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own comments");

        // Delete all replies to this comment
        await _commentRepository.DeleteRepliesByCommentIdAsync(request.CommentId);

        // Delete the comment
        await _commentRepository.DeleteAsync(request.CommentId);
    }
}
