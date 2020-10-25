namespace Paillave.Etl.Connector
{
    public interface IFileValueConnectors
    {
        IFileValueProvider GetProvider(string code);
        IFileValueProcessor GetProcessor(string code);
    }
}