using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Interfaces;

namespace Rekindle.Memories.Application.Groups.Commands;

public record MergeImageGroupUserCommand(Guid GroupId, Guid SourceUserId, Guid TargetUserId) : IRequest;

public class MergeImageGroupUserCommandHandler : IRequestHandler<MergeImageGroupUserCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IImageSearchClient _imageSearchClient;

    public MergeImageGroupUserCommandHandler(IGroupRepository groupRepository, IImageSearchClient imageSearchClient)
    {
        _groupRepository = groupRepository;
        _imageSearchClient = imageSearchClient;
    }

    public async Task Handle(MergeImageGroupUserCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindByIdAsync(request.GroupId, cancellationToken);

        if (group is null)
        {
            throw new GroupNotFoundException();
        }

        if (group.TempUsers.Any(u => u.Id == request.TargetUserId))
        {
            throw new InvalidOperationException("Cannot merge into a temporary user.");
        }

        if (group.Members.All(u => u.Id != request.TargetUserId))
        {
            throw new InvalidOperationException("Target user must be a member of the group.");
        }

        await _imageSearchClient.MergeUsersAsync(
            request.GroupId,
            request.SourceUserId,
            request.TargetUserId,
            cancellationToken);

        var sourceUser = group.Members.FirstOrDefault(u => u.Id == request.SourceUserId);
        if (sourceUser is not null)
        {
            group.Members.Remove(sourceUser);
            var targetUser = group.Members.FirstOrDefault(u => u.Id == request.TargetUserId);
            if (targetUser is not null)
            {
                targetUser.LastFaceFileId = sourceUser.LastFaceFileId;
            }
        }
        else
        {
            var tempUser = group.TempUsers.FirstOrDefault(u => u.Id == request.SourceUserId);
            if (tempUser is not null)
            {
                group.TempUsers.Remove(tempUser);
                var targetUser = group.Members.FirstOrDefault(u => u.Id == request.TargetUserId);
                if (targetUser is not null)
                {
                    targetUser.LastFaceFileId = tempUser.LastFaceFileId;
                }
            }
        }

        await _groupRepository.ReplaceAsync(group, cancellationToken);
    }
}