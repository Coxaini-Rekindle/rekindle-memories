using MediatR;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Application.Common.Messaging;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Memories.Requests;
using Rekindle.Memories.Application.Storage.Interfaces;
using Rekindle.Memories.Contracts;
using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Commands.CreateMemory;

public record CreateMemoryCommand : IRequest<MemoryDto>
{
    public Guid GroupId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<CreateImageRequest> Images { get; init; } = [];
    public Guid CreatorUserId { get; init; }
}

public class CreateMemoryCommandHandler : IRequestHandler<CreateMemoryCommand, MemoryDto>
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly IPostRepository _postRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IEventPublisher _eventPublisher;

    public CreateMemoryCommandHandler(
        IMemoryRepository memoryRepository,
        IPostRepository postRepository,
        IGroupRepository groupRepository,
        IFileStorage fileStorage, IEventPublisher eventPublisher)
    {
        _memoryRepository = memoryRepository;
        _postRepository = postRepository;
        _groupRepository = groupRepository;
        _fileStorage = fileStorage;
        _eventPublisher = eventPublisher;
    }

    public async Task<MemoryDto> Handle(CreateMemoryCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindById(request.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.CreatorUserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }

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
                ParticipantIds = []
            });
        }

        var memory = Memory.Create(
            groupId: request.GroupId,
            title: request.Title,
            description: request.Description,
            creatorUserId: request.CreatorUserId,
            mainPostId: Guid.Empty
        );

        var post = Post.Create(
            memoryId: memory.Id,
            content: request.Content,
            images: images,
            creatorUserId: request.CreatorUserId
        );
        memory.SetMainPost(post.Id);
        await _memoryRepository.InsertMemory(memory, cancellationToken);
        await _postRepository.InsertPost(post, cancellationToken);

        await _eventPublisher.PublishAsync(new PostCreatedEvent()
        {
            MemoryId = memory.Id,
            GroupId = memory.GroupId,
            PostId = post.Id,
            UserId = request.CreatorUserId,
            Images = post.Images.Select(i => i.FileId).ToList(),
            Title = request.Title,
            Content = request.Content,
        });

        return memory.ToDto(post);
    }
}