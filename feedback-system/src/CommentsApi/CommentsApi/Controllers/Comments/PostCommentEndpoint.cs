using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Ardalis.ApiEndpoints;

using Microsoft.AspNetCore.Mvc;

using ProgrammerAl.CommentsApi.DB.Repositories;

using Swashbuckle.AspNetCore.Annotations;

namespace ProgrammerAl.CommentsApi.Controllers.Comments;

public class PostCommentEndpoint : EndpointBaseAsync
    .WithRequest<PostCommentEndpointRequest>
    .WithActionResult<PostCommentEndpointResponse>
{
    private readonly ICommentsRepository _commentsRepository;

    public PostCommentEndpoint(ICommentsRepository commentsRepository)
    {
        _commentsRepository = commentsRepository;
    }

    [HttpPost("/add-comment")]
    [SwaggerOperation(
        Summary = "Adds a comment",
        Description = "Adds a comment",
        OperationId = nameof(PostCommentEndpoint),
        Tags = new[] { "Comments" })
    ]
    public override async Task<ActionResult<PostCommentEndpointResponse>> HandleAsync(PostCommentEndpointRequest request, CancellationToken cancellationToken = default)
    {
        await _commentsRepository.StoreNewCommentAsync(request.Comments);
        return NoContent();
    }
}

public class PostCommentEndpointRequest
{
    [Required(AllowEmptyStrings = false), NotNull]
    public string? Comments { get; set; }
}

public record PostCommentEndpointResponse();
