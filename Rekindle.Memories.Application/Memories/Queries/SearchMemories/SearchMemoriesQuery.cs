using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Interfaces;

namespace Rekindle.Memories.Application.Memories.Queries.SearchMemories;

public record SearchMemoriesQuery(
    Guid GroupId,
    Guid UserId,
    string? SearchTerm,
    ulong Limit,
    ulong Offset
) : IRequest<IEnumerable<SearchMemoryResponse>>;

public class SearchMemoriesQueryHandler : IRequestHandler<SearchMemoriesQuery, IEnumerable<SearchMemoryResponse>>
{
    private readonly IImageSearchClient _imageSearchClient;
    private readonly IMemoryPostRepository _memoryPostRepository;
    private readonly IGroupRepository _groupRepository;

    public SearchMemoriesQueryHandler(
        IImageSearchClient imageSearchClient,
        IMemoryPostRepository memoryPostRepository,
        IGroupRepository groupRepository)
    {
        _imageSearchClient = imageSearchClient;
        _memoryPostRepository = memoryPostRepository;
        _groupRepository = groupRepository;
    }

    public async Task<IEnumerable<SearchMemoryResponse>> Handle(SearchMemoriesQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindById(request.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        }

        var searchResults = await _imageSearchClient.SearchImages(
            request.GroupId,
            request.SearchTerm ?? string.Empty,
            request.Limit,
            request.Offset,
            cancellationToken);

        var memoryIds = searchResults.Select(r => r.MemoryId).Distinct().ToList();

        var memoriesWithMainPosts =
            await _memoryPostRepository.GetMemoriesWithMainPostsByIds(memoryIds, cancellationToken);
        var memoriesLookup = memoriesWithMainPosts.ToDictionary(m => m.Memory.Id, m => m);

        var results = searchResults.Select(r =>
        {
            var memoryWithMainPost = memoriesLookup.GetValueOrDefault(r.MemoryId);
            var mainPost = memoryWithMainPost?.MainPost;

            return new SearchMemoryResponse(
                r.MemoryId,
                r.CreatedAt,
                new MemoryMatchedPhoto(
                    r.PhotoId,
                    r.PostId,
                    r.Title,
                    r.Content,
                    r.PublisherUserId
                ),
                mainPost?.Content ?? memoryWithMainPost?.Memory.Title ?? "",
                mainPost?.Content ?? memoryWithMainPost?.Memory.Description ?? ""
            );
        }).ToList();

        return results;
    }
}