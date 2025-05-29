using MediatR;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.UpdateComment;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _commentRepository;

    public UpdateCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.FindByIdAsync(request.CommentId);
        if (comment == null)
            throw new CommentNotFoundException();

        // Verify the user owns the comment
        if (comment.CreatorUserId != request.UserId)
            throw new UnauthorizedAccessException("You can only update your own comments");

        // Update the comment
        comment.UpdateContent(request.Content);
        await _commentRepository.UpdateAsync(comment);

        return comment.ToDto(request.UserId);
    }
}