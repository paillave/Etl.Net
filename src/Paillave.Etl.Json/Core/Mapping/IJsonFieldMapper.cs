namespace Paillave.Etl.Json.Core.Mapping
{
    public interface IJsonFieldMapper
    {
        T ToPathQuery<T>(string xPathQuery);
        T ToPathQuery<T>(string xPathQuery, int depthScope);
        string ToSourceName();
        Guid ToRowGuid();
    }
}
