using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
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

using static PulumiInfra.Builders.StaticSiteResources;

using AzureNative = Pulumi.AzureNative;

namespace PulumiInfra.Builders;

public record StaticSiteResources(SiteStorageInfra StorageInfra)
{
    public record SiteStorageInfra(StorageAccount StireStorageAccount, StorageAccountStaticWebsite StaticSiteAccount);
}

public record StaticSiteBuilder(
    GlobalConfig GlobalConfig,
    ResourceGroup ResourceGroup)
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
                Name = AzureNative.Storage.SkuName.Standard_GRS,
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

        var fullSearchPath = Path.GetFullPath(GlobalConfig.StaticSiteConfig.StaticSitePath);
        foreach (var fullFilePath in Directory.GetFiles(fullSearchPath, searchPattern: "*.*", SearchOption.AllDirectories))
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
                BlobName = relativeFilePath,
            });
        }

        return new SiteStorageInfra(storageAccount, siteStorageAccount);
    }
}
