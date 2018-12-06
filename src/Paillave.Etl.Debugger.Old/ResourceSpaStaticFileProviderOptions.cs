using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Paillave.Etl.Debugger
{
    public class ResourceSpaStaticFileProviderOptions
    {
        public string DevelopmentRootPath { get; set; }
        public string ReleaseRootPath { get; set; }
    }
}