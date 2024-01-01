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

using Pulumi.AzureStaticWebsite;

namespace PulumiInfra.Builders;

public record StaticSiteResources()
{
}

public record StaticSiteBuilder(
    GlobalConfig GlobalConfig,
    ResourceGroup ResourceGroup)
{
    public StaticSiteResources Build()
    {
        var website = new Website("static-site", new WebsiteArgs
        {
            SitePath = GlobalConfig.StaticSiteConfig.StaticSitePath,
            DomainResourceGroup = ResourceGroup.Name
        }, new ComponentResourceOptions
        { 
            Provider = new Pulumi.AzureStaticWebsite.Provider("static-site-provider", args: new Pulumi.AzureStaticWebsite.ProviderArgs
            { 
                
            })
        });

        //var staticSite = new StaticSite("static-site", new()
        //{
        //    Location = ResourceGroup.Location,
        //    ResourceGroupName = ResourceGroup.Name,
        //    Sku = new SkuDescriptionArgs
        //    {
        //        Name = "Basic",
        //        Tier = "Basic",
        //    },
        //    BuildProperties = new StaticSiteBuildPropertiesArgs
        //    { 

        //    }
        //});
        return new StaticSiteResources();
    }
}
