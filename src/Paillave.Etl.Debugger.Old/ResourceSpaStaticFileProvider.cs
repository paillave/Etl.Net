using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Reflection;

namespace Paillave.Etl.Debugger
{
    internal class ResourceSpaStaticFileProvider : ISpaStaticFileProvider
    {
        private IFileProvider _fileProvider;

        public ResourceSpaStaticFileProvider(IServiceProvider serviceProvider, ResourceSpaStaticFileProviderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            if (env.IsDevelopment() || string.IsNullOrWhiteSpace(options.ReleaseRootPath))
            {
                if (string.IsNullOrEmpty(options.DevelopmentRootPath))
                    throw new ArgumentException($"The {nameof(options.DevelopmentRootPath)} property of {nameof(options)} cannot be null or empty.");

                var absoluteRootPath = Path.Combine(env.ContentRootPath, options.DevelopmentRootPath);

                if (Directory.Exists(absoluteRootPath))
                    _fileProvider = new PhysicalFileProvider(absoluteRootPath);
            }
            else
                _fileProvider = new ManifestEmbeddedFileProvider(this.GetType().Assembly, options.ReleaseRootPath);
        }

        public IFileProvider FileProvider => _fileProvider;
    }
}