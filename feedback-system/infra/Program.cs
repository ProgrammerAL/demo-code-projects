using Pulumi;
using Pulumi.AzureNative.Resources;

using PulumiInfra;
using PulumiInfra.Builders;
using PulumiInfra.Config;

using System.Collections.Generic;

return await Pulumi.Deployment.RunAsync(async () =>
{
    var pulumiConfig = new Config();
    var globalConfig = await GlobalConfig.LoadAsync(pulumiConfig);

    var resourceGroup = new ResourceGroup(globalConfig.ApiConfig.ResourceGroupName, new ResourceGroupArgs
    {
        Location = globalConfig.ApiConfig.Location
    });

    var apiBuilder = new ApiBuilder(globalConfig, resourceGroup);
    var apiResources = apiBuilder.Build();

    var staticSiteBuilder = new StaticSiteBuilder(globalConfig, resourceGroup, apiResources);
    _ = staticSiteBuilder.Build();

    return new Dictionary<string, object?>
    {
        { "Readme", Output.Create(System.IO.File.ReadAllText("./Pulumi.README.md")) },
        { "FunctionHttpsEndpoint", apiResources.Function.HttpsEndpoint },
    };
});

