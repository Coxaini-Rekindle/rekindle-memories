using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Domain;
using Rekindle.Search.Contracts;

namespace Rekindle.Memories.Application.Users.EventHandlers;

public class ImageFacesAnalyzedEventHandler : IHandleMessages<ImageFacesAnalyzedEvent>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IPostRepository _postRepository;

    public ImageFacesAnalyzedEventHandler(IGroupRepository groupRepository, IPostRepository postRepository)
    {
        _groupRepository = groupRepository;
        _postRepository = postRepository;
    }

    public async Task Handle(ImageFacesAnalyzedEvent message)
    {
        var post = await _postRepository.FindByIdAsync(message.PostId);

        if (post is null)
        {
            throw new PostNotFoundException();
        }

        var group = await _groupRepository.FindByIdAsync(message.GroupId);

        if (group is null)
        {
            throw new GroupNotFoundException();
        }

        var image = post.Images.FirstOrDefault(i => i.FileId == message.ImageId);

        if (image is null)
        {
            throw new ImageNotFoundException();
        }

        image.RecognizedUserIds = message.Users.Select(u => u.UserId).ToList();
        image.TempUserIds = message.TempUser.Select(u => u.UserId).ToList();
        foreach (var user in message.Users)
        {
            var groupMember = group.Members.FirstOrDefault(m => m.Id == user.UserId);
            if (groupMember is not null)
            {
                groupMember.LastFaceFileId = user.FaceFileId;
            }
        }

        foreach (var tempUsers in message.TempUser)
        {
            var tempMember = group.TempUsers.FirstOrDefault(u => u.Id == tempUsers.UserId);

            if (tempMember is not null)
            {
                tempMember.LastFaceFileId = tempUsers.FaceFileId;
            }
            else
            {
                group.TempUsers.Add(new TempUser
                {
                    Id = tempUsers.UserId,
                    LastFaceFileId = tempUsers.FaceFileId
                });
            }
        }

        await _postRepository.ReplaceAsync(post);
        await _groupRepository.ReplaceAsync(group);
    }
}