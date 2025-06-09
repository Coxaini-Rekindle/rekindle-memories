using System.Net.Http.Json;
using Rekindle.Memories.Application.Memories.Interfaces;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Infrastructure.Search;

public class ImageSearchClient : IImageSearchClient
{
    private readonly HttpClient _client;

    public ImageSearchClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<PhotoSearchResult>> SearchImagesAsync(
        Guid groupId,
        string query,
        IEnumerable<Guid> participants,
        ulong limit,
        ulong offset,
        CancellationToken cancellationToken = default)
    {
        var participantsQuery = participants.Any()
            ? string.Join("&", participants.Select(p => $"participants={p}"))
            : string.Empty;

        var response = await _client.GetFromJsonAsync<IEnumerable<PhotoSearchResult>>(
            $"groups/{groupId}/search?query={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}&{participantsQuery}",
            cancellationToken);

        if (response == null)
        {
            throw new HttpRequestException("Failed to retrieve search results.");
        }

        return response;
    }

    public async Task MergeUsersAsync(
        Guid groupId,
        Guid userId,
        Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            UserId = userId,
            TargetUserId = targetUserId
        };

        var response = await _client.PostAsJsonAsync(
            $"groups/{groupId}/users/merge",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to merge users: {response.ReasonPhrase}");
        }
    }
}