using System.Collections.Generic;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.ValuesProviders
{
    public interface IFileValueWithDestinationMetadata : IFileValueMetadata
    {
        Dictionary<string, Destination> Destinations { get; set; }
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