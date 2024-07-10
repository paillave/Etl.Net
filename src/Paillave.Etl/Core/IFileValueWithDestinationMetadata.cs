using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public interface IFileValueWithDestinationMetadata : IFileValueMetadata
    {
        Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
    }
    public class Destination
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string ZipCode { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Culture {get;set;}
        
    }
}