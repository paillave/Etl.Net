namespace Paillave.Etl.XmlFile.Core.Mapping.Visitors
{
    public class DummyFieldMapper : IXmlFieldMapper
    {
        public XmlFieldDefinition MappingSetter { get; } = new XmlFieldDefinition();

        public T ToXPathQuery<T>(string xPathQuery)
        {
            this.MappingSetter.XPathQuery = xPathQuery;
            return default;
        }
    }
}