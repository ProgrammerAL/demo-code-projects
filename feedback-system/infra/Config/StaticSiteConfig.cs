using System;

namespace PulumiInfra.Config;
public record StaticSiteConfig(
    string StaticSitePath);

public class StaticSiteConfigDto : ConfigDtoBase<StaticSiteConfig>
{
    public string? StaticSitePath { get; set; }

    public override StaticSiteConfig GenerateValidConfigObject()
    {
        if (!string.IsNullOrWhiteSpace(StaticSitePath))
        {
            return new(StaticSitePath);
        }

        throw new Exception($"{GetType().Name} has invalid config");
    }
}
