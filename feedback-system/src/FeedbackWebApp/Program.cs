using FeedbackWebApp;
using FeedbackWebApp.Configs;
using FeedbackWebApp.HttpClients;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureConfig<ApiConfig>(builder);

builder.Services.AddHttpClient<IFeedbackHttpClient, FeedbackHttpClient>((serviceProvider, client) =>
{
    var apiConfig = serviceProvider.GetRequiredService<ApiConfig>();

    var baseEndpoint = apiConfig.FeedbackEndpoint;
    if (!baseEndpoint.EndsWith('/'))
    {
        baseEndpoint += "/";
    }

    client.BaseAddress = new Uri(baseEndpoint);
});

await builder.Build().RunAsync();


static T ConfigureConfig<T>(WebAssemblyHostBuilder builder)
    where T : class, new()
{
    var config = new T();
    builder.Configuration.Bind(typeof(T).Name, config);
    _ = builder.Services.AddSingleton(config);
    return config;
}