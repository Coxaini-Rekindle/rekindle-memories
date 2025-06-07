using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.CreateComment;

public record CreateCommentCommand(
    Guid MemoryId,
    string Content,
    Guid UserId,
    Guid? ReplyToPostId = null,
    Guid? ReplyToCommentId = null
) : IRequest<CommentDto>;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
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
        }

        // If replying to a post, verify post belongs to the memory
        if (request.ReplyToPostId.HasValue)
        {
            var replyPost = await _postRepository.FindByIdAsync(request.ReplyToPostId.Value);
            if (replyPost == null || replyPost.MemoryId != request.MemoryId)
            {
                throw new PostNotFoundException();
            }
        }

        // If replying to a comment, verify comment exists and belongs to the memory
        if (request.ReplyToCommentId.HasValue)
        {
            var parentComment = await _commentRepository.FindByIdAsync(request.ReplyToCommentId.Value);
            if (parentComment == null || parentComment.MemoryId != request.MemoryId)
            {
                throw new CommentNotFoundException();
            }
        }

        // Create comment
        var comment = Comment.Create(
            memoryId: request.MemoryId,
            content: request.Content,
            creatorUserId: request.UserId,
            replyToPostId: request.ReplyToPostId,
            replyToCommentId: request.ReplyToCommentId);
        await _commentRepository.InsertComment(comment, cancellationToken);

        return comment.ToDto(request.UserId);
    }
}