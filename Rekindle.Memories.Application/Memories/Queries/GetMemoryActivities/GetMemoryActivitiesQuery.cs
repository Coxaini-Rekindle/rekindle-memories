using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetMemoryActivities;

public record GetMemoryActivitiesQuery(
    Guid MemoryId,
    int PageSize = 20,
    DateTime? Cursor = null,
    Guid UserId = default
) : IRequest<CursorPaginationResponse<MemoryActivityDto>>;
