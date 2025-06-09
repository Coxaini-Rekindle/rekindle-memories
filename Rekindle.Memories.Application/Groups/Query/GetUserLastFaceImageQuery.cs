using MediatR;
using Rekindle.Memories.Application.Storage.Models;

namespace Rekindle.Memories.Application.Groups.Query;

public record GetUserLastFaceImageQuery(
    Guid GroupId,
    Guid UserId,
    Guid RequestingUserId
) : IRequest<FileResponse>;
