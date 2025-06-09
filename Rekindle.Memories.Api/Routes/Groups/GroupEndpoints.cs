using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Rekindle.Memories.Api.Helpers;
using Rekindle.Memories.Application.Groups.Commands;
using Rekindle.Memories.Application.Groups.Models;
using Rekindle.Memories.Application.Groups.Query;

namespace Rekindle.Memories.Api.Routes.Groups;

public static class GroupEndpoints
{
    public static IEndpointRouteBuilder MapGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var groupEndpoint = app.MapGroup("groups/{groupId:guid}")
            .WithTags("Groups")
            .WithDisplayName("Groups")
            .WithDescription("Endpoints for managing groups and their members")
            .RequireAuthorization();

        groupEndpoint.MapGet("users",
                async (Guid groupId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var command = new GetImageGroupUsersQuery(groupId);

                    var results = await mediator.Send(command, cancellationToken);

                    return Results.Ok(results);
                })
            .WithName("GetGroupUsers")
            .WithDescription("Get users in a group")
            .Produces<IEnumerable<ImageGroupUserDto>>();

        groupEndpoint.MapGet("users/{userId:guid}/face-image",
                async (
                    [FromRoute] Guid groupId,
                    [FromRoute] Guid userId,
                    [FromServices] IMediator mediator,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken) =>
                {
                    var requestingUserId = ClaimsHelper.GetUserIdFromClaims(user);

                    var query = new GetUserLastFaceImageQuery(
                        GroupId: groupId,
                        UserId: userId,
                        RequestingUserId: requestingUserId
                    );

                    var result = await mediator.Send(query, cancellationToken);
                    return Results.Stream(result.Stream, result.ContentType);
                })
            .WithName("GetUserLastFaceImage")
            .WithDescription("Get the last face image of a user or temp user in the group");

        groupEndpoint.MapPost("users/merge",
            async (
                [FromRoute] Guid groupId,
                [FromBody] MergeUserRequest request,
                [FromServices] IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new MergeImageGroupUserCommand(
                    GroupId: groupId,
                    SourceUserId: request.SourceUserId,
                    TargetUserId: request.TargetUserId
                );
                await mediator.Send(command, cancellationToken);
                return Results.Ok();
            });

        return app;
    }

    private record MergeUserRequest(
        Guid SourceUserId,
        Guid TargetUserId
    );
}