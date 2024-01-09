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
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;

using PulumiInfra.Config;

using AzureNative = Pulumi.AzureNative;

namespace PulumiInfra.Builders;

public record PersistentStorageResources(PersistentStorageResources.PersistentStorageInfra StorageInfra)
{
    public record PersistentStorageInfra(StorageAccount StorageAccount);
}

public record PersistentStorageBuilder(
    GlobalConfig GlobalConfig,
    ResourceGroup ResourceGroup)
{
    public PersistentStorageResources Build()
    {
        var storageInfra = GenerateStorageInfrastructure();
        return new PersistentStorageResources(storageInfra);
    }

    private PersistentStorageResources.PersistentStorageInfra GenerateStorageInfrastructure()
    {
        var storageAccount = new StorageAccount("persistentstg", new StorageAccountArgs
        {
            ResourceGroupName = ResourceGroup.Name,
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
            }
        });

        return new PersistentStorageResources.PersistentStorageInfra(storageAccount);
    }
}
