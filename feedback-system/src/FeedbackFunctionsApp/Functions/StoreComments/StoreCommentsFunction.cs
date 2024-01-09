using System.Net;
using System.Text;

using Azure.Data.Tables;
using Azure.Identity;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;

namespace FeedbackFunctionsApp.Functions.StoreComment;

public class StoreCommentsFunction
{
    private readonly IOptions<StorageConfig> _storageConfig;

    public StoreCommentsFunction(IOptions<StorageConfig> storageConfig)
    {
        _storageConfig = storageConfig;
    }

    [Function("store-comments")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var requestDto = await req.ReadFromJsonAsync<StoreCommentsRequestDto>();
        if (requestDto is null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var requestObject = requestDto.GenerateValidObject();
        var itemKey = Guid.NewGuid().ToString();

        var tableUri = new Uri(_storageConfig.Value.Endpoint);
        var tableClient = new TableClient(tableUri, _storageConfig.Value.TableName, new DefaultAzureCredential());

        await tableClient.CreateIfNotExistsAsync();

        // Make a dictionary entity by defining a <see cref="TableEntity">.
        var tableEntity = new TableEntity(itemKey, itemKey)
        {
            { "Comments", requestObject.Comments },
        };

        await tableClient.AddEntityAsync(tableEntity);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
