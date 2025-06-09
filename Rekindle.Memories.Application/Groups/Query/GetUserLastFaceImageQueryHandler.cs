using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Storage.Interfaces;
using Rekindle.Memories.Application.Storage.Models;

namespace Rekindle.Memories.Application.Groups.Query;

public class GetUserLastFaceImageQueryHandler : IRequestHandler<GetUserLastFaceImageQuery, FileResponse>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IFileStorage _fileStorage;

    public GetUserLastFaceImageQueryHandler(
        IGroupRepository groupRepository,
        IFileStorage fileStorage)
    {
        _groupRepository = groupRepository;
        _fileStorage = fileStorage;
    }

    public async Task<FileResponse> Handle(GetUserLastFaceImageQuery request, CancellationToken cancellationToken)
    {
        // Get the group to verify it exists and check user permissions
        var group = await _groupRepository.FindByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        // Check if the requesting user is a member of the group
        var isRequestingUserMember = group.Members.Any(m => m.Id == request.RequestingUserId);
        if (!isRequestingUserMember)
        {
            throw new UserNotGroupMemberException();
        }

        // Find the target user (regular member or temp user)
        var targetUser = group.Members.FirstOrDefault(m => m.Id == request.UserId);
        var targetTempUser = group.TempUsers.FirstOrDefault(tu => tu.Id == request.UserId);

        Guid? lastFaceFileId = null;

        if (targetUser != null)
        {
            lastFaceFileId = targetUser.LastFaceFileId;
        }
        else if (targetTempUser != null)
        {
            lastFaceFileId = targetTempUser.LastFaceFileId;
        }
        else
        {
            throw new UserNotFoundException();
        }

        // Check if the user has a last face image
        if (lastFaceFileId == null)
        {
            throw new ImageNotFoundException();
        }

        // Get the file from storage
        return await _fileStorage.GetAsync(lastFaceFileId.Value, cancellationToken);
    }
}
