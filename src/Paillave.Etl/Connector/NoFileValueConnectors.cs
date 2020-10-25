namespace Paillave.Etl.Connector
{
    public class NoFileValueConnectors : IFileValueConnectors
    {
        public IFileValueProvider GetProvider(string code) => new NoFileValueProvider(code);
        public IFileValueProcessor GetProcessor(string code) => new NoFileValueProcessor(code);
    }
}