using System;
using System.Collections.Generic;
using Paillave.Etl.Connector;

namespace Paillave.Etl.FromConfigurationConnectors
{
    public partial class ConfigurationFileValueConnectorParser
    {
        private class ConfigurationFileConnectors : IFileValueConnectors
        {
            private Dictionary<string, IFileValueProcessor> _processors
                = new Dictionary<string, IFileValueProcessor>();
            private Dictionary<string, IFileValueProvider> _providers
                = new Dictionary<string, IFileValueProvider>();

            public ConfigurationFileConnectors(Dictionary<string, IFileValueProvider> providers, Dictionary<string, IFileValueProcessor> processors)
            {
                _processors = processors;
                _providers = providers;
            }
            public IFileValueProcessor GetProcessor(string code)
                => this._processors.TryGetValue(code, out var proc) ? proc : new NoFileValueProcessor(code);
            public IFileValueProvider GetProvider(string code)
                => this._providers.TryGetValue(code, out var proc) ? proc : new NoFileValueProvider(code);
        }
    }
}