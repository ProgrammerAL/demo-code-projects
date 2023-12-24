using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;

using Ardalis.ApiEndpoints;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ProgrammerAl.CommentsApi.Controllers.Comments;

public class PostCommentEndpoint : EndpointBaseSync
    .WithRequest<PostCommentEndpoint.StoreCommentsRequest>
    .WithActionResult<PostCommentEndpoint.ResponseObj>
{
    private readonly IOptions<StorageConfig> _storageConfig;

    public PostCommentEndpoint(IOptions<StorageConfig> storageConfig)
    {
        _storageConfig = storageConfig;
    }

    public override ActionResult<ResponseObj> Handle(StoreCommentsRequest request)
    {
        var itemKey = Guid.NewGuid().ToString();

        var tableClient = new TableClient(_storageConfig.Value.Endpoint, _storageConfig.Value.TableName);

        await tableClient.CreateIfNotExistsAsync();

        // Make a dictionary entity by defining a <see cref="TableEntity">.
        var tableEntity = new TableEntity(itemKey, itemKey)
        {
            { "Comments", requestObject.Comments },
        };

        await tableClient.AddEntityAsync(tableEntity);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    public class StoreCommentsRequest
    {
        [FromBody, Required(AllowEmptyStrings = false), NotNull]
        public string? Comments { get; set; }
    }
    public record ResponseObj(string Comments);

}
