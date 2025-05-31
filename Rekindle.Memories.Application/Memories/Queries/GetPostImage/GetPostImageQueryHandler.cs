using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Storage.Interfaces;
using Rekindle.Memories.Application.Storage.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetPostImage;

public class GetPostImageQueryHandler : IRequestHandler<GetPostImageQuery, FileResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IFileStorage _fileStorage;

    public GetPostImageQueryHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository,
        IFileStorage fileStorage)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
        _fileStorage = fileStorage;
    }    public async Task<FileResponse> Handle(GetPostImageQuery request, CancellationToken cancellationToken)
    {
        // First, check if the user is authorized to view any images from this post
        // We can do this with a direct query instead of fetching 3 separate entities
        
        // Check if the post exists and contains this image
        var post = await _postRepository.FindById(request.PostId, cancellationToken);
        if (post == null)
        {
            throw new PostNotFoundException();
        }

        // Verify the image belongs to this post
        var image = post.Images.FirstOrDefault(img => img.FileId == request.ImageFileId);
        if (image == null)
        {
            throw new FileNotFoundException($"Image with ID {request.ImageFileId} not found in post {request.PostId}");
        }
        
        // Check if user is a member of the group in a single query without loading whole entities
        var isUserAuthorized = await IsUserAuthorizedToAccessPost(post.MemoryId, request.UserId, cancellationToken);
        if (!isUserAuthorized)
        {
            throw new UserNotGroupMemberException();
        }

        // If validation passes, get the file directly from storage
        return await _fileStorage.GetAsync(request.ImageFileId, cancellationToken);
    }
    
    private async Task<bool> IsUserAuthorizedToAccessPost(Guid memoryId, Guid userId, CancellationToken cancellationToken)
    {
        // Get the memory to find its group
        var memory = await _memoryRepository.FindById(memoryId, cancellationToken);
        if (memory == null)
        {
            return false;
        }
        
        // Check if the user is in the group directly without loading all members
        var userGroups = await _groupRepository.FindByUserId(userId, cancellationToken);
        return userGroups.Any(g => g.Id == memory.GroupId);
    }
}
