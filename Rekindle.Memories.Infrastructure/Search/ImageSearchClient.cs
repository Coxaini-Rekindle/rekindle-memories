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

    public async Task<IEnumerable<PhotoSearchResult>> SearchImages(
        Guid groupId,
        string query,
        ulong limit,
        ulong offset,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetFromJsonAsync<IEnumerable<PhotoSearchResult>>(
            $"groups/{groupId}/search?query={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}",
            cancellationToken);

        if (response == null)
        {
            throw new HttpRequestException("Failed to retrieve search results.");
        }

        return response;
    }
}