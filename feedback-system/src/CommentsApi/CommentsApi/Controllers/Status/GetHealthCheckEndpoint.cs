using Ardalis.ApiEndpoints;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.Annotations;

namespace ProgrammerAl.CommentsApi.Controllers.Status;

public class GetHealthCheckEndpoint : EndpointBaseSync
    .WithoutRequest
    .WithActionResult<GetHealthCheckEndpointResponse>
{
    private readonly IOptions<ServiceConfig> _serviceConfig;

    public GetHealthCheckEndpoint(IOptions<ServiceConfig> serviceConfig)
    {
        _serviceConfig = serviceConfig;
    }

    [HttpPost("/health")]
    [SwaggerOperation(
        Summary = "Perform a health check",
        Description = "Used to check this API is running",
        OperationId = nameof(GetHealthCheckEndpoint),
        Tags = new[] { "Status" })
    ]
    public override ActionResult<GetHealthCheckEndpointResponse> Handle()
    {
        var responseObject = new GetHealthCheckEndpointResponse(Version: _serviceConfig.Value.Version, Environment: _serviceConfig.Value.Environment, ResponseTime: DateTime.UtcNow);
        return Ok(responseObject);
    }
}

public record GetHealthCheckEndpointResponse(string Version, string Environment, DateTime ResponseTime);
