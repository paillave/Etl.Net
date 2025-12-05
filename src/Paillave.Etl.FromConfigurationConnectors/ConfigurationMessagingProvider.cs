using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Paillave.Etl.Core;

namespace Paillave.Etl.FromConfigurationConnectors;

public class ConfigurationMessagingProvider(IConfiguration configuration, IEnumerable<IMessagingProvider> messagingProviders)
{
    public IMessaging GetMessaging(string sectionName)
    {
        IConfigurationSection configurationSection = configuration.GetSection(sectionName);
        var type = configurationSection.GetSection("Type").Value;

        var messagingProvider = messagingProviders.Single(m => m.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase));
        return messagingProvider.GetMessaging(configurationSection.GetSection("Properties") ?? throw new InvalidOperationException("No configuration properties found for messaging provider"));
    }
}
