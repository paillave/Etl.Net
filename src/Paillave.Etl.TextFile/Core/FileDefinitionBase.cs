namespace Paillave.Etl.TextFile.Core
{
    public abstract class FileDefinitionBase<T> where T : new()
    {
        public abstract LineSerializer<T> GetSerializer();
    }
}