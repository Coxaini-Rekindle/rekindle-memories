namespace Rekindle.Memories.Application.Groups.Models;

public record ImageGroupUserDto(Guid UserId, Guid? LastFaceFileId, bool IsTemp);