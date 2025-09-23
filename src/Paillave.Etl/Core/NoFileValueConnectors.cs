namespace Paillave.Etl.Core
{
    public class NoFileValueConnectors : IFileValueConnectors
    {
        public string[] Processors => [];
        public string[] Providers => [];

        public IFileValueProvider GetProvider(string code) => new NoFileValueProvider(code);
        public IFileValueProcessor GetProcessor(string code) => new NoFileValueProcessor(code);
    }
}