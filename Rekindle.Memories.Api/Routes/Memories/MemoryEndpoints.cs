using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Rekindle.Memories.Api.Models;
using Rekindle.Memories.Application.Memories.Commands.CreateMemory;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Memories.Queries.GetMemories;
using Rekindle.Memories.Application.Memories.Queries.GetMemoryById;
using Rekindle.Memories.Application.Memories.Requests;

namespace Rekindle.Memories.Api.Routes.Memories;

public static class MemoryEndpoints
{
    public static IEndpointRouteBuilder MapMemoryEndpoints(this IEndpointRouteBuilder app)
    {
        var groupsEndpoint = app.MapGroup("groups/{groupId:guid}/memories")
            .WithTags("Memories")
            .WithDisplayName("Memories")
            .WithDescription("Endpoints for managing memories")
            .RequireAuthorization();

        var memoriesEndpoint = app.MapGroup("memories")
            .WithTags("Memories")
            .WithDisplayName("Memories")
            .WithDescription("Endpoints for managing memories")
            .RequireAuthorization();

        groupsEndpoint.MapPost("/", CreateMemory)
            .WithName("CreateMemory")
            .WithSummary("Create a new memory")
            .WithDescription("Creates a new memory in a group with an initial post and images")
            .Accepts<CreateMemoryFormRequest>("multipart/form-data")
            .Produces<MemoryDto>(201)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404)
            .Produces(500)
            .WithOpenApi(operation =>
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Description = "Memory data with optional file uploads",
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["title"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Memory title"
                                    },
                                    ["description"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Memory description (optional)"
                                    },
                                    ["content"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Memory content"
                                    },
                                    ["images"] = new OpenApiSchema
                                    {
                                        Type = "array",
                                        Items = new OpenApiSchema
                                        {
                                            Type = "string",
                                            Format = "binary"
                                        },
                                        Description = "Image files to upload"
                                    },
                                    ["existingFileIds"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description =
                                            "JSON array of existing file IDs to include (e.g., [\"guid1\",\"guid2\"])"
                                    }
                                },
                                Required = new HashSet<string> { "title", "content" }
                            }
                        }
                    }
                };
                return operation;
            })
            .DisableAntiforgery();

        groupsEndpoint.MapGet("/", GetMemories)
            .WithName("GetMemories")
            .WithSummary("Get memories with cursor pagination")
            .WithDescription("Gets memories from a group with cursor-based pagination")
            .Produces<CursorPaginationResponse<MemoryDto>>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        memoriesEndpoint.MapGet("/{memoryId:guid}", GetMemoryById)
            .WithName("GetMemoryById")
            .WithSummary("Get a specific memory")
            .WithDescription("Gets a specific memory by its ID")
            .Produces<MemoryDto>(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> CreateMemory(
        [FromRoute] Guid groupId,
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
                return Results.BadRequest(
                    "Request must be multipart/form-data for file uploads");
            }

            var form = await httpRequest.ReadFormAsync();
            // Extract basic memory information from form
            var title = form["title"].FirstOrDefault();
            var description = form["description"].FirstOrDefault() ?? string.Empty;
            var content = form["content"].FirstOrDefault();

            if (string.IsNullOrEmpty(title))
            {
                return Results.BadRequest("Title is required");
            }

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
            foreach (var fileId in existingFileIds)
            {
                images.Add(new CreateImageRequest
                {
                    FileId = fileId
                });
            }

            // Add new uploaded files
            var imageFiles = form.Files.Where(f => f.Name == "images").ToList();
            foreach (var file in imageFiles)
            {
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

            var command = new CreateMemoryCommand
            {
                GroupId = groupId,
                Title = title,
                Description = description,
                Content = content,
                Images = images,
                CreatorUserId = userId
            };

            var result = await mediator.Send(command);
            return Results.Created($"/groups/{groupId}/memories/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Error processing request: {ex.Message}");
        }
    }

    private static async Task<IResult> GetMemories(
        [FromRoute] Guid groupId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        [FromQuery] int limit = 20,
        [FromQuery] DateTime? cursor = null)
    {
        var userId = GetUserIdFromClaims(user);

        var query = new GetMemoriesQuery
        {
            GroupId = groupId,
            Limit = Math.Min(limit, 100), // Cap at 100 items per request
            Cursor = cursor,
            UserId = userId
        };

        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMemoryById(
        [FromRoute] Guid memoryId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user)
    {
        var userId = GetUserIdFromClaims(user);

        var query = new GetMemoryByIdQuery
        {
            MemoryId = memoryId,
            UserId = userId
        };

        var result = await mediator.Send(query);
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