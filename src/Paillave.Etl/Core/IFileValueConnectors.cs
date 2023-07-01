namespace Paillave.Etl.Core
{
    public interface IFileValueConnectors
    {
        IFileValueProvider GetProvider(string code);
        IFileValueProcessor GetProcessor(string code);
    }
}