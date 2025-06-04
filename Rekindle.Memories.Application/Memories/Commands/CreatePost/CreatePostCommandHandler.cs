using MediatR;
using Rekindle.Memories.Application.Common.Messaging;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Storage.Interfaces;
using Rekindle.Memories.Contracts;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IEventPublisher _eventPublisher;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository,
        IFileStorage fileStorage, IEventPublisher eventPublisher)
    {
        _postRepository = postRepository;
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
        _fileStorage = fileStorage;
        _eventPublisher = eventPublisher;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
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

        // Process images
        var images = new List<Image>();
        foreach (var imageRequest in request.Images)
        {
            Guid fileId;

            if (imageRequest.FileStream != null && !string.IsNullOrEmpty(imageRequest.ContentType))
            {
                fileId = await _fileStorage.UploadAsync(imageRequest.FileStream, imageRequest.ContentType,
                    cancellationToken);
            }
            else if (imageRequest.FileId.HasValue)
            {
                fileId = imageRequest.FileId.Value;
            }
            else
            {
                throw new ArgumentException(
                    "Either FileStream and ContentType or FileId must be provided for each image.");
            }

            images.Add(new Image
            {
                FileId = fileId,
                ParticipantIds = imageRequest.ParticipantIds ?? []
            });
        }

        // Create post
        var post = Post.Create(
            memoryId: request.MemoryId,
            content: request.Content,
            images: images,
            creatorUserId: request.UserId
        );

        await _postRepository.InsertPost(post, cancellationToken);

        await _eventPublisher.PublishAsync(new PostCreatedEvent()
        {
            MemoryId = memory.Id,
            GroupId = memory.GroupId,
            PostId = post.Id,
            UserId = request.UserId,
            Images = post.Images.Select(i => i.FileId).ToList(),
            Title = post.Content,
            Content = request.Content,
        });

        return post.ToDto(request.UserId);
    }
}