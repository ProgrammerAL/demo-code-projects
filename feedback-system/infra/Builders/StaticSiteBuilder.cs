using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Pulumi;
using Pulumi.AzureNative;
using Pulumi.AzureNative.AppConfiguration;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

using PulumiInfra.Config;
using PulumiInfra.Utilities;

using static PulumiInfra.Builders.StaticSiteResources;

using AzureNative = Pulumi.AzureNative;

namespace PulumiInfra.Builders;

public record StaticSiteResources(SiteStorageInfra StorageInfra)
{
    public record SiteStorageInfra(StorageAccount SiteStorageAccount, StorageAccountStaticWebsite StaticSiteAccount);
}

public record StaticSiteBuilder(
    GlobalConfig GlobalConfig,
    ResourceGroup ResourceGroup,
    ApiResources ApiResources)
{
    public StaticSiteResources Build()
    {
        var siteStorageInfra = GenerateSiteInfra();
        return new StaticSiteResources(siteStorageInfra);
    }

    private SiteStorageInfra GenerateSiteInfra()
    {
        var storageAccount = new StorageAccount("sitestorage", new StorageAccountArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            AllowBlobPublicAccess = true,
            Sku = new AzureNative.Storage.Inputs.SkuArgs
            {
                Name = AzureNative.Storage.SkuName.Standard_LRS,
            },
            Kind = Kind.StorageV2,
            EnableHttpsTrafficOnly = true,
            MinimumTlsVersion = MinimumTlsVersion.TLS1_2,
            AccessTier = AccessTier.Hot,
            AllowSharedKeyAccess = true,
            SasPolicy = new AzureNative.Storage.Inputs.SasPolicyArgs
            {
                ExpirationAction = ExpirationAction.Log,
                SasExpirationPeriod = "00.01:00:00"
            },
        });

        var siteStorageAccount = new StorageAccountStaticWebsite("staticsiteaccount", new StorageAccountStaticWebsiteArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            IndexDocument = "index.html",
            Error404Document = "index.html",
            AccountName = storageAccount.Name
        });

        _ = ApiResources.Function.HttpsEndpoint.Apply(apiEndpoint =>
        {
            var appsettingsFilePath = $"{GlobalConfig.StaticSiteConfig.StaticSitePath}/appsettings.json";
            WriteConfigFiles(appsettingsFilePath, apiEndpoint);

            //Create Pulumi entries for each file to upload
            var fullSearchPath = Path.GetFullPath(GlobalConfig.StaticSiteConfig.StaticSitePath);
            var allFiles = Directory.EnumerateFiles(fullSearchPath, searchPattern: "*", SearchOption.AllDirectories);
            foreach (var fullFilePath in allFiles)
            {
                var relativeFilePath = fullFilePath.Substring(fullSearchPath.Length).Trim('/').Trim('\\');
                var fileName = Path.GetFileName(fullFilePath);
                _ = new Blob(fileName, new BlobArgs
                {
                    AccountName = storageAccount.Name,
                    ContainerName = "$web",
                    AccessTier = BlobAccessTier.Hot,
                    ResourceGroupName = ResourceGroup.Name,
                    Source = new FileAsset(fullFilePath),
                    ContentType = FileUtilities.DetermineFileContentType(fullFilePath),
                    BlobName = relativeFilePath,
                });
            }

            //Need to return something even though we don't use the returned value
            return "";
        });


        return new SiteStorageInfra(storageAccount, siteStorageAccount);
    }

    private void WriteConfigFiles(string appsettingsFilePath, string apiEndpoint)
    {
        //Write a new appsettings.json file for the blazor client
        JsonNode appSettingsJson = JsonNode.Parse("{}")!;
        AddApiConfigValues(appSettingsJson, apiEndpoint);

        var contents = StringContentUtilities.GenerateCompressedStringContent(appSettingsJson);

        File.WriteAllText($"{appsettingsFilePath}", contents.Content);
        File.WriteAllBytes($"{appsettingsFilePath}.br", contents.BrotliContent);
        File.WriteAllBytes($"{appsettingsFilePath}.gz", contents.GZipContent);
    }

    private void AddApiConfigValues(JsonNode appSettingsJson, string apiEndpoint)
    {
        var jsonObject = new JsonObject
        {
            ["FeedbackEndpoint"] = $"{apiEndpoint}/api",
        };

        appSettingsJson["ApiConfig"] = jsonObject;
    }
}
