using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;

namespace Rekindle.Memories.Application.Memories.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;

    public DeletePostCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new PostNotFoundException();
        }

        // Verify the user owns the post
        if (post.CreatorUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
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

        // Delete all comments associated with this post
        await _commentRepository.DeleteByPostIdAsync(request.PostId);

        // Delete the post
        await _postRepository.DeletePost(request.PostId, cancellationToken);
    }
}