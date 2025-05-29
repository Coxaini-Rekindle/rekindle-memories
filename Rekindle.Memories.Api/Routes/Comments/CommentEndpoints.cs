using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rekindle.Memories.Application.Memories.Commands.AddCommentReaction;
using Rekindle.Memories.Application.Memories.Commands.CreateComment;
using Rekindle.Memories.Application.Memories.Commands.UpdateComment;
using Rekindle.Memories.Application.Memories.Commands.DeleteComment;
using Rekindle.Memories.Application.Memories.Commands.RemoveCommentReaction;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Memories.Queries.GetMemoryActivities;

namespace Rekindle.Memories.Api.Routes.Comments;

public static class CommentEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var commentsEndpoint = app.MapGroup("comments")
            .WithTags("Comments")
            .WithDisplayName("Comments")
            .WithDescription("Endpoints for managing comments on posts")
            .RequireAuthorization();        var memoryCommentsEndpoint = app.MapGroup("memories/{memoryId:guid}/activities")
            .WithTags("Memory Activities")
            .WithDisplayName("Memory Activities")
            .WithDescription("Endpoints for managing activities (posts and comments) within specific memories")
            .RequireAuthorization();

        // Comment CRUD operations
        memoryCommentsEndpoint.MapPost("/comments", CreateComment)
            .WithName("CreateComment")
            .WithSummary("Create a new comment in a memory")
            .WithDescription("Creates a new comment in a memory or as a reply to another comment or post")
            .Accepts<CreateCommentRequest>("application/json")
            .Produces<CommentDto>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        memoryCommentsEndpoint.MapGet("/", GetMemoryActivities)
            .WithName("GetMemoryActivities")
            .WithSummary("Get activities from a memory")
            .WithDescription("Gets all activities (posts and comments) from a specific memory with cursor-based pagination")
            .Produces<CursorPaginationResponse<MemoryActivityDto>>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        commentsEndpoint.MapPut("/{commentId:guid}", UpdateComment)
            .WithName("UpdateComment")
            .WithSummary("Update a comment")
            .WithDescription("Updates a comment's content (only the owner can update)")
            .Accepts<UpdateCommentRequest>("application/json")
            .Produces<CommentDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        commentsEndpoint.MapDelete("/{commentId:guid}", DeleteComment)
            .WithName("DeleteComment")
            .WithSummary("Delete a comment")
            .WithDescription("Deletes a comment and all its replies (only the owner can delete)")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Comment reactions
        commentsEndpoint.MapPost("/{commentId:guid}/reactions", AddCommentReaction)
            .WithName("AddCommentReaction")
            .WithSummary("Add or update reaction to a comment")
            .WithDescription("Adds or updates a reaction to a comment")
            .Accepts<AddReactionRequest>("application/json")
            .Produces<ReactionSummaryDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        commentsEndpoint.MapDelete("/{commentId:guid}/reactions", RemoveCommentReaction)
            .WithName("RemoveCommentReaction")
            .WithSummary("Remove reaction from a comment")
            .WithDescription("Removes user's reaction from a comment")
            .Produces<ReactionSummaryDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }    private static async Task<IResult> CreateComment(
        [FromRoute] Guid memoryId,
        [FromBody] CreateCommentRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new CreateCommentCommand(
            MemoryId: memoryId,
            Content: request.Content,
            UserId: userId,
            ReplyToPostId: request.ReplyToPostId,
            ReplyToCommentId: request.ReplyToCommentId
        );

        var result = await mediator.Send(command);
        return Results.Created($"/comments/{result.Id}", result);
    }

    private static async Task<IResult> GetMemoryActivities(
        [FromRoute] Guid memoryId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? cursor = null)
    {
        var userId = GetUserIdFromClaims(user);

        var query = new GetMemoryActivitiesQuery(
            MemoryId: memoryId,
            PageSize: Math.Min(pageSize, 100), // Cap at 100
            Cursor: cursor,
            UserId: userId
        );

        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateComment(
        [FromRoute] Guid commentId,
        [FromBody] UpdateCommentRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new UpdateCommentCommand(
            CommentId: commentId,
            Content: request.Content,
            UserId: userId
        );

        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteComment(
        [FromRoute] Guid commentId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new DeleteCommentCommand(
            CommentId: commentId,
            UserId: userId
        );

        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> AddCommentReaction(
        [FromRoute] Guid commentId,
        [FromBody] AddReactionRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);
        var command = new AddCommentReactionCommand
        {
            CommentId = commentId,
            ReactionType = request.Type,
            UserId = userId
        };

        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveCommentReaction(
        [FromRoute] Guid commentId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);
        var command =
            new RemoveCommentReactionCommand
            {
                CommentId = commentId,
                UserId = userId
            };

        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value
                          ?? user.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format in token");
        }

        return userId;
    }
}