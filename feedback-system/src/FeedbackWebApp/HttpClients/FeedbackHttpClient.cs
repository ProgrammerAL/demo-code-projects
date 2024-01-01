using System.Text;
using System.Text.Json;

namespace FeedbackWebApp.HttpClients;

public interface IFeedbackHttpClient
{ 
    ValueTask<HttpResponseMessage> SubmitCommentsAsync(string comments);
}

public class FeedbackHttpClient : IFeedbackHttpClient
{
    private readonly HttpClient _httpClient;

    public FeedbackHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<HttpResponseMessage> SubmitCommentsAsync(string comments)
    {
        var requestObject = new StoreCommentsRequest(comments);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "store-comments")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestObject), Encoding.UTF8, "application/json")
        };

        return await _httpClient.SendAsync(requestMessage);
    }

    private record StoreCommentsRequest(string Comments);
}
