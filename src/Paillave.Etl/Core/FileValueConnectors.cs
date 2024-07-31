using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class FileValueConnectors : IFileValueConnectors
    {
        private Dictionary<string, IFileValueProcessor> _processors
            = new Dictionary<string, IFileValueProcessor>();
        private Dictionary<string, IFileValueProvider> _providers
            = new Dictionary<string, IFileValueProvider>();
        public string[] Processors => _processors.Keys.ToArray();
        public string[] Providers => _providers.Keys.ToArray();
        public FileValueConnectors Register(IFileValueProvider provider)
        {
            this._providers[provider.Code] = provider;
            return this;
        }
        public FileValueConnectors Register(IFileValueProcessor processor)
        {
            this._processors[processor.Code] = processor;
            return this;
        }
        public FileValueConnectors() { }
        public FileValueConnectors(Dictionary<string, IFileValueProvider> providers, Dictionary<string, IFileValueProcessor> processors)
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