using FeedbackFunctionsApp;
using FeedbackFunctionsApp.Middleware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerApplication =>
    {
        workerApplication.UseMiddleware<ExceptionHandlerMiddleware>();
    })
    .ConfigureAppConfiguration(x =>
    {
#if DEBUG
        x.AddJsonFile("host.json");
        x.AddJsonFile("local.settings.json");
#endif
    })
    .ConfigureServices(serviceCollection =>
    {
        AddConfigOptions<ServiceConfig>(serviceCollection);
        AddConfigOptions<StorageConfig>(serviceCollection);
    })
    .Build();

host.Run();


static void AddConfigOptions<TOptions>(IServiceCollection serviceCollection)
    where TOptions : class
{
    serviceCollection.AddOptions<TOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection(typeof(TOptions).Name).Bind(settings);
    });
}
