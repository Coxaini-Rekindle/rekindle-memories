using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rekindle.Memories.Api.Helpers;
using Rekindle.Memories.Application.Memories.Queries.SearchMemories;

namespace Rekindle.Memories.Api.Routes.Search;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var searchEndpoint = app.MapGroup("groups/{groupId:guid}/search")
            .WithTags("Search")
            .WithDisplayName("Search")
            .WithDescription("Endpoints for searching memories and content")
            .RequireAuthorization();

        searchEndpoint.MapGet("/memories", SearchMemories)
            .WithName("SearchMemories")
            .WithSummary("Search memories by content")
            .WithDescription("Searches memories in a group based on image content and text")
            .Produces<IEnumerable<SearchMemoryResponse>>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> SearchMemories(
        [FromRoute] Guid groupId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        [FromQuery] Guid[]? participants,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0
    )
    {
        var userId = ClaimsHelper.GetUserIdFromClaims(user);

        var query = new SearchMemoriesQuery(
            groupId,
            userId,
            searchTerm,
            (ulong)Math.Min(limit, 100), // Limit max results
            (ulong)Math.Max(offset, 0), // Ensure non-negative offset
            participants ?? []
        );

        var result = await mediator.Send(query);
        return Results.Ok(result);
    }
}