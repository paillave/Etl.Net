using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Paillave.Etl.Debugger.Hubs;

namespace Paillave.Etl.Debugger
{
    // https://github.com/aspnet/JavaScriptServices/blob/master/src/Microsoft.AspNetCore.SpaServices.Extensions/StaticFiles/SpaStaticFilesExtensions.cs
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();
            services.AddTransient<ISpaStaticFileProvider, ResourceSpaStaticFileProvider>();
            services.AddTransient(i => new ResourceSpaStaticFileProviderOptions
            {
                DevelopmentRootPath = "ClientApp/build",
                ReleaseRootPath = "ClientApp/build"
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ISpaStaticFileProvider spaProvider)
        {
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            options.FileProvider = spaProvider.FileProvider;
            app.UseDefaultFiles(options);
            app.UseSpaStaticFiles();

            // app.UseHttpsRedirection();

            app.UseSignalR(routes =>
                        {
                            routes.MapHub<EtlProcessDebugHub>("/EtlProcessDebug");
                        });
            app.UseMvc();

            if (env.IsDevelopment())
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    spa.UseReactDevelopmentServer(npmScript: "start");
                });
        }
    }
}
