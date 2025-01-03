namespace Paillave.Etl.Core
{
    public class NoFileValueConnectors : IFileValueConnectors
    {
        public string[] Processors => new string[] { };
        public string[] Providers => new string[] { };

        public IFileValueProvider GetProvider(string code) => new NoFileValueProvider(code);
        public IFileValueProcessor GetProcessor(string code) => new NoFileValueProcessor(code);
    }
}