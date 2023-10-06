

using Microsoft.Extensions.Options;
using Slimsy.DependencyInjection;

namespace Umbraco.FotoPatricia
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web hosting environment.</param>
        /// <param name="config">The configuration.</param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337.
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddOptions();
            services.Configure<CachingOptions>(_config.GetSection(nameof(CachingOptions)));

            services.AddTransient<IConfigureOptions<StaticFileOptions>, ConfigureStaticFileOptions>();

            services.AddResponseCompression(options => options.EnableForHttps = true);
            services.AddResponseCaching();

            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder =>
                {
                    builder
                        .Cache()
                        .Expire(TimeSpan.FromMinutes(5));

                    builder
                        .With(context => context.HttpContext.Request.Path.StartsWithSegments("/media"))
                        .Cache()
                        .Expire(TimeSpan.FromHours(6));
                    
                    builder
                        .With(context => context.HttpContext.Request.Path.StartsWithSegments("/scripts"))
                        .Cache()
                        .Expire(TimeSpan.FromHours(6));
                });
            });

            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddComposers()
                .AddSlimsy()
                .Build();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSecurityHeaders();
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var cachingOptions = app.ApplicationServices.GetRequiredService<IOptions<CachingOptions>>();
            if (cachingOptions.Value.UseResponseCaching)
            {
                app.UseResponseCaching();
            }

            if (cachingOptions.Value.UseOutputCaching)
            {
                app.UseOutputCache();
            }

            app.UseHealthChecks("/healthz");
            
            app.UseUmbraco()
                .WithMiddleware(u =>
                {
                    u.UseBackOffice();
                    u.UseWebsite();
                })
                .WithEndpoints(u =>
                {
                    u.UseInstallerEndpoints();
                    u.UseBackOfficeEndpoints();
                    u.UseWebsiteEndpoints();
                });
        }
    }

    public class CachingOptions
    {
        public bool UseResponseCaching { get; set; }
        public bool UseOutputCaching { get; set; }
    }
}
