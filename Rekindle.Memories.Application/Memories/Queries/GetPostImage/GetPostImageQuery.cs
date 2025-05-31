using MediatR;
using Rekindle.Memories.Application.Storage.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetPostImage;

public record GetPostImageQuery(
    Guid PostId,
    Guid ImageFileId,
    Guid UserId
) : IRequest<FileResponse>;
