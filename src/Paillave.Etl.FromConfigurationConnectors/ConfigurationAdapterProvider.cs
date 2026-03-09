using Microsoft.Extensions.Configuration;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors;

public class ConfigurationAdapterProvider(IConfiguration configuration, ConfigurationFileValueConnectorParser parser)
{
    private const string code = "DEVITEMSOURCE";
    public IFileValueProvider GetFileValueProvider(string sectionName)
    {
        IConfigurationSection configurationSection = configuration.GetSection(sectionName);
        return parser.GetProvider(configurationSection);
    }
    public IFileValueProcessor GetFileValueProcessor(string sectionName)
    {
        IConfigurationSection configurationSection = configuration.GetSection(sectionName);
        return parser.GetProcessor(configurationSection);
    }
}
