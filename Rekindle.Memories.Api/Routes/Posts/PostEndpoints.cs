using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rekindle.Memories.Api.Models;
using Rekindle.Memories.Application.Memories.Commands.CreatePost;
using Rekindle.Memories.Application.Memories.Commands.UpdatePost;
using Rekindle.Memories.Application.Memories.Commands.DeletePost;
using Rekindle.Memories.Application.Memories.Commands.AddReaction;
using Rekindle.Memories.Application.Memories.Commands.RemoveReaction;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Memories.Queries.GetPostById;
using Rekindle.Memories.Application.Memories.Queries.GetPostImage;
using Rekindle.Memories.Application.Memories.Queries.GetMemoryActivities;
using Rekindle.Memories.Application.Memories.Requests;

namespace Rekindle.Memories.Api.Routes.Posts;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var postsEndpoint = app.MapGroup("posts")
            .WithTags("Posts")
            .WithDisplayName("Posts")
            .WithDescription("Endpoints for managing posts within memories")
            .RequireAuthorization();

        var memoryPostsEndpoint = app.MapGroup("memories/{memoryId:guid}/posts")
            .WithTags("Posts")
            .WithDisplayName("Memory Posts")
            .WithDescription("Endpoints for managing posts within specific memories")
            .RequireAuthorization();


        memoryPostsEndpoint.MapPost("/", CreatePostForm)
            .WithName("CreatePostForm")
            .WithSummary("Create a new post with file uploads")
            .WithDescription("Creates a new post within a specific memory using multipart form data for file uploads")
            .Accepts<CreatePostFormRequest>("multipart/form-data")
            .Produces<PostDto>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        memoryPostsEndpoint.MapGet("/", GetPostsByMemory)
            .WithName("GetPostsByMemory")
            .WithSummary("Get posts from a memory")
            .WithDescription("Gets posts from a specific memory with cursor-based pagination")
            .Produces<CursorPaginationResponse<PostDto>>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);
        postsEndpoint.MapGet("/{postId:guid}", GetPostById)
            .WithName("GetPostById")
            .WithSummary("Get a specific post")
            .WithDescription("Gets a specific post by its ID")
            .Produces<PostDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        postsEndpoint.MapGet("/{postId:guid}/images/{imageFileId:guid}", GetPostImage)
            .WithName("GetPostImage")
            .WithSummary("Get a post image")
            .WithDescription("Gets a specific image from a post")
            .Produces<FileResult>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        postsEndpoint.MapPut("/{postId:guid}", UpdatePost)
            .WithName("UpdatePost")
            .WithSummary("Update a post")
            .WithDescription("Updates a post's content (only the owner can update)")
            .Accepts<UpdatePostRequest>("application/json")
            .Produces<PostDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        postsEndpoint.MapDelete("/{postId:guid}", DeletePost)
            .WithName("DeletePost")
            .WithSummary("Delete a post")
            .WithDescription("Deletes a post and all its comments (only the owner can delete)")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Post reactions
        postsEndpoint.MapPost("/{postId:guid}/reactions", AddPostReaction)
            .WithName("AddPostReaction")
            .WithSummary("Add or update reaction to a post")
            .WithDescription("Adds or updates a reaction to a post")
            .Accepts<AddReactionRequest>("application/json")
            .Produces<ReactionSummaryDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        postsEndpoint.MapDelete("/{postId:guid}/reactions", RemovePostReaction)
            .WithName("RemovePostReaction")
            .WithSummary("Remove reaction from a post")
            .WithDescription("Removes user's reaction from a post")
            .Produces<ReactionSummaryDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> CreatePostForm(
        [FromRoute] Guid memoryId,
        HttpRequest httpRequest,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        try
        {
            // Check if the request contains multipart form data
            if (!httpRequest.HasFormContentType)
            {
                return Results.BadRequest("Request must be multipart/form-data for file uploads");
            }

            var form = await httpRequest.ReadFormAsync();

            // Extract content from form
            var content = form["content"].FirstOrDefault();
            if (string.IsNullOrEmpty(content))
            {
                return Results.BadRequest("Content is required");
            }

            // Process existing file IDs (if provided)
            var existingFileIdsJson = form["existingFileIds"].FirstOrDefault();
            List<Guid> existingFileIds = [];

            if (!string.IsNullOrEmpty(existingFileIdsJson))
            {
                try
                {
                    existingFileIds = JsonSerializer.Deserialize<List<Guid>>(existingFileIdsJson) ?? [];
                }
                catch (JsonException)
                {
                    return Results.BadRequest("Invalid existingFileIds format. Expected JSON array of GUIDs.");
                }
            }


            // Process uploaded image files and existing files
            var images = new List<CreateImageRequest>();

            // Add existing files first
            for (int i = 0; i < existingFileIds.Count; i++)
            {
                images.Add(new CreateImageRequest
                {
                    FileId = existingFileIds[i]
                });
            }

            // Add new uploaded files
            var imageFiles = form.Files.Where(f => f.Name == "images").ToList();
            for (int i = 0; i < imageFiles.Count; i++)
            {
                var file = imageFiles[i];
                if (file.Length > 0)
                {
                    images.Add(new CreateImageRequest
                    {
                        FileStream = file.OpenReadStream(),
                        ContentType = file.ContentType,
                        FileName = file.FileName
                    });
                }
            }

            var command = new CreatePostCommand(
                MemoryId: memoryId,
                Content: content,
                Images: images,
                UserId: userId
            );

            var result = await mediator.Send(command);
            return Results.Created($"/posts/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error processing request: {ex.Message}");
        }
    }

    private static async Task<IResult> GetPostsByMemory(
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

        // Filter to only posts and convert to PostDto
        var postDtos = result.Items
            .Where(activity => activity.Type == MemoryActivityType.Post)
            .Select(activity => new PostDto
            {
                Id = activity.Id,
                MemoryId = activity.MemoryId,
                Content = activity.Content,
                Images = activity.Images,
                CreatedAt = activity.CreatedAt,
                CreatorUserId = activity.CreatorUserId,
                Reactions = activity.Reactions,
                ReactionSummary = activity.ReactionSummary
            })
            .ToList();

        var postsResult = new CursorPaginationResponse<PostDto>
        {
            Items = postDtos,
            NextCursor = result.NextCursor,
            HasMore = result.HasMore
        };

        return Results.Ok(postsResult);
    }

    private static async Task<IResult> GetPostById(
        [FromRoute] Guid postId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var query = new GetPostByIdQuery(
            PostId: postId,
            UserId: userId
        );

        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePost(
        [FromRoute] Guid postId,
        [FromBody] UpdatePostRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new UpdatePostCommand(
            PostId: postId,
            Content: request.Content,
            UserId: userId
        );
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPostImage(
        [FromRoute] Guid postId,
        [FromRoute] Guid imageFileId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var query = new GetPostImageQuery(
            PostId: postId,
            ImageFileId: imageFileId,
            UserId: userId
        );

        var result = await mediator.Send(query);
        return Results.Stream(result.Stream, result.ContentType);
    }

    private static async Task<IResult> DeletePost(
        [FromRoute] Guid postId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new DeletePostCommand(
            PostId: postId,
            UserId: userId
        );

        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> AddPostReaction(
        [FromRoute] Guid postId,
        [FromBody] AddReactionRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new AddPostReactionCommand
        {
            PostId = postId,
            ReactionType = request.Type,
            UserId = userId
        };

        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> RemovePostReaction(
        [FromRoute] Guid postId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var command = new RemovePostReactionCommand
        {
            PostId = postId,
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

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }
}