using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryAnalyticsApi.InternalModel;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;

namespace RepositoryAnalyticsApi
{
    public class Startup
    {
        private const string appName = "Repo Analytics API";
        private const int appVersion = 1;
        private IHostingEnvironment env;
        private IConfiguration configuration;
        readonly string CorsPolicy = "CorsPolicy";

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                 .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                 .Enrich.FromLogContext()
                 .WriteTo.Console()
                 .CreateLogger();

            var flattenedAppSettings = new FlattenedAppSettings();
            configuration.Bind(flattenedAppSettings);

            // Map the flattened dependencies in to the structured ones until the whole "__" not working in docker
            // compose thing can be figured out
            var dependencySettings = new InternalModel.AppSettings.Dependencies
            {
                Database = new InternalModel.AppSettings.Database
                {
                    ConnectionString = flattenedAppSettings.DatabaseConnectionString
                },
                GitHub = new InternalModel.AppSettings.GitHub
                {
                    GraphQlApiUrl = flattenedAppSettings.GitHubGraphQlApiUrl,
                    V3ApiUrl = flattenedAppSettings.GitHubV3ApiUrl
                },
                Redis = new InternalModel.AppSettings.Redis
                {
                    Configuration = flattenedAppSettings.RedisConfiguration,
                    InstanceName = flattenedAppSettings.RedisInstanceName
                }
            };

            Log.Logger.Information("Dependency Configuration = {@dependencySettings}", dependencySettings);

            var cachingSettings = new InternalModel.AppSettings.Caching
            {
                Durations = new InternalModel.AppSettings.CacheDurations
                {
                    DevOpsIntegrations = flattenedAppSettings.CachingDurationDevOpsIntegrations,
                    OrganizationTeams = flattenedAppSettings.CachingDurationOrganizationTeams,
                    OwnerType = flattenedAppSettings.CachingDurationOwnerType,
                    RepositoryData = flattenedAppSettings.CachingDurationRepositoryData,
                    DependencyNameSearchResults = flattenedAppSettings.CachingDurationDependencyNameSearchResults
                }
            };

            Log.Logger.Information("Caching Configuration = {@cachingSettings}", cachingSettings);

            if (dependencySettings == null)
            {
                throw new ArgumentException("Unable to find Dependency configuration!!!");
            }

            services.AddSingleton(typeof(InternalModel.AppSettings.Dependencies), dependencySettings);

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            ContainerManager.RegisterServices(services, configuration ,env, dependencySettings, cachingSettings);
            ContainerManager.RegisterExtensions(services, configuration);

            services.AddMvc().AddJsonOptions(options =>
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            );

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = dependencySettings.Redis.Configuration;
                options.InstanceName = dependencySettings.Redis.InstanceName;
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = appName, Version = $"v{appVersion}" });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "RepositoryAnaltyicsApi.Controllers.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Setup DB schema
            using (var serviceScope = app.ApplicationServices
                   .GetRequiredService<IServiceScopeFactory>()
                   .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<RepositoryAnalysisContext>())
                {
                    // Wipe the db and schema
                    //context.Database.EnsureDeleted();

                    Log.Logger.Information("Initializing Database");

                    context.Database.EnsureCreated();
                    context.ManualConfiguration();

                    Log.Logger.Information("Database Succesfully Initialized");
                    
                    // Note should eventually be using the below line once everything is solidified
                    // context.Database.Migrate();
                }
            }

            app.UseCors(CorsPolicy);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} V{appVersion}");
            });

            app.UseMvc();
        }
    }
}
