using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

using System.Net;
using System.Reflection;

namespace FeedbackFunctionsApp.Middleware;

public class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var req = await context.GetHttpRequestDataAsync();
            var response = req!.CreateResponse();
            await response.WriteAsJsonAsync(ex.ToString(), HttpStatusCode.InternalServerError);

            SetHttpResponseData(context, response);
        }
    }

    private static void SetHttpResponseData(FunctionContext context, HttpResponseData responseData)
    {
        var keyValuePair = context.Features.FirstOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
        object functionBindingsFeature = keyValuePair.Value;
        if (functionBindingsFeature != null)
        {
            PropertyInfo? pinfo = functionBindingsFeature.GetType()?.GetProperty("InvocationResult");
            pinfo?.SetValue(functionBindingsFeature, responseData);
        }
    }
}
