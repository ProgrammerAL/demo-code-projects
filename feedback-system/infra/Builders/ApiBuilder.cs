using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

using AzureNative = Pulumi.AzureNative;

namespace PulumiInfra.Builders;

public record ApiResources(ApiResources.ServiceStorageInfra ServiceStorage, ApiResources.FunctionInfra Function)
{
    public record ServiceStorageInfra(StorageAccount StorageAccount, BlobContainer FunctionsContainer, Blob FunctionsBlob);
    public record FunctionInfra(WebApp WebApp, Output<string> HttpsEndpoint);
}

public record ApiBuilder(
    GlobalConfig GlobalConfig,
    ResourceGroup ResourceGroup,
    PersistentStorageResources PersistenceResources)
{
    public ApiResources Build()
    {
        var storageInfra = GenerateStorageInfrastructure();
        var functionsInfra = GenerateFunctionsInfrastructure(storageInfra);
        AssignRbacAccesses(functionsInfra, storageInfra);

        return new ApiResources(storageInfra, functionsInfra);
    }

    private ApiResources.ServiceStorageInfra GenerateStorageInfrastructure()
    {
        var storageAccount = new StorageAccount("funcsstorage", new StorageAccountArgs
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

        //Storage Container to host the Azure Functions
        var functionsContainer = new BlobContainer("functions-container", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            PublicAccess = PublicAccess.None,
            ResourceGroupName = ResourceGroup.Name,
        });

        var functionsBlob = new Blob("functions-blob", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = functionsContainer.Name,
            AccessTier = BlobAccessTier.Hot,
            ResourceGroupName = ResourceGroup.Name,
            Source = new FileArchive(GlobalConfig.ApiConfig.FunctionsPackagePath),
            BlobName = "functions.zip",
        });

        return new ApiResources.ServiceStorageInfra(
            storageAccount,
            functionsContainer,
            functionsBlob);
    }

    private ApiResources.FunctionInfra GenerateFunctionsInfrastructure(ApiResources.ServiceStorageInfra storageInfra)
    {
        //Create the App Service Plan
        var appServicePlan = new AppServicePlan("functions-app-service-plan", new AppServicePlanArgs
        {
            ResourceGroupName = ResourceGroup.Name,
            Kind = "Linux",
            Sku = new SkuDescriptionArgs
            {
                Tier = "Dynamic",
                Name = "Y1"
            },
            // For Linux, you need to change the plan to have Reserved = true property.
            Reserved = true,
        });

        var tableEndpoint = PersistenceResources.StorageInfra.StorageAccount.Name.Apply(x => $"https://{x}.table.core.windows.net");

        var functionAppSiteConfig = new SiteConfigArgs
        {
            LinuxFxVersion = "DOTNET-ISOLATED|8.0",
            Cors = new CorsSettingsArgs
            {
                AllowedOrigins = new[] { "*" }
            },
            AppSettings = new[]
            {
                new NameValuePairArgs{
                    Name = "AzureWebJobsStorage__accountname",
                    Value = storageInfra.StorageAccount.Name
                },
                new NameValuePairArgs{
                    Name = "WEBSITE_RUN_FROM_PACKAGE",
                    Value = storageInfra.FunctionsBlob.Url
                },
                new NameValuePairArgs
                {
                    Name = "FUNCTIONS_WORKER_RUNTIME",
                    Value = "dotnet-isolated",
                },
                new NameValuePairArgs
                {
                    Name = "FUNCTIONS_EXTENSION_VERSION",
                    Value = "~4",
                },
                new NameValuePairArgs
                {
                    Name = "SCM_DO_BUILD_DURING_DEPLOYMENT",
                    Value = "0"
                },
                new NameValuePairArgs
                {
                    Name = "ServiceConfig__Version",
                    Value = GlobalConfig.ServiceConfig.Version
                },
                new NameValuePairArgs
                {
                    Name = "ServiceConfig__Environment",
                    Value = GlobalConfig.ServiceConfig.Environment,
                },
                new NameValuePairArgs
                {
                    Name = "StorageConfig__Endpoint",
                    Value = tableEndpoint,
                },
                new NameValuePairArgs
                {
                    Name = "StorageConfig__TableName",
                    Value = "Comments",
                }
            }
        };

        //Create the App Service
        var webApp = new WebApp("functions-app", new WebAppArgs
        {
            Kind = "FunctionApp",
            ResourceGroupName = ResourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            HttpsOnly = true,
            SiteConfig = functionAppSiteConfig,
            ClientAffinityEnabled = false,
            Identity = new ManagedServiceIdentityArgs
            {
                Type = AzureNative.Web.ManagedServiceIdentityType.SystemAssigned,
            },
        });

        var httpsEndpoint = webApp.DefaultHostName.Apply(x => $"https://{x}");

        return new ApiResources.FunctionInfra(
            webApp,
            httpsEndpoint);
    }

    private void AssignRbacAccesses(ApiResources.FunctionInfra functionsInfra, ApiResources.ServiceStorageInfra storageInfra)
    {
        var functionPrincipalId = functionsInfra.WebApp.Identity.Apply(x => x!.PrincipalId);
        var blobOwnerRoleDefinitionId = GenerateStorageBlobDataOwnerRoleId(GlobalConfig.ApiConfig.ClientConfig.SubscriptionId);

        //Allow reading of the Storage Container that stores the Functions Zip Package
        //Note: Even though the function app only reads from the storage, it needs read/write access. I don't know why
        _ = new RoleAssignment("funcs-storage-blob-data-reader-role-assignment", new RoleAssignmentArgs
        {
            PrincipalId = functionPrincipalId,
            PrincipalType = PrincipalType.ServicePrincipal,
            RoleDefinitionId = blobOwnerRoleDefinitionId,
            Scope = storageInfra.StorageAccount.Id
        });


        var tableContributorRoleId = GenerateStorageTableContributorRoleId(GlobalConfig.ApiConfig.ClientConfig.SubscriptionId);

        //Allow the Azure Function Managed Identity to be able to read/write to table storage on this account
        _ = new RoleAssignment("funcs-storage-table-contributor-role-assignment", new RoleAssignmentArgs
        {
            PrincipalId = functionPrincipalId,
            PrincipalType = PrincipalType.ServicePrincipal,
            RoleDefinitionId = tableContributorRoleId,
            Scope = PersistenceResources.StorageInfra.StorageAccount.Id
        });
    }

    private static string GenerateStorageBlobDataOwnerRoleId(string subscriptionId)
    {
        return "/subscriptions/" + subscriptionId + "/providers/Microsoft.Authorization/roleDefinitions/b7e6dc6d-f1e8-4753-8033-0f276bb0955b";
    }

    private static string GenerateStorageTableContributorRoleId(string subscriptionId)
    {
        return "/subscriptions/" + subscriptionId + "/providers/Microsoft.Authorization/roleDefinitions/0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3";
    }
}
