using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Configuration;
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
            // Load config data 
            var dependencies = configuration.GetSection("Dependencies").Get<InternalModel.AppSettings.Dependencies>();

            if (dependencies == null)
            {
                throw new ArgumentException("Unable to find Dependency configuration!!!");
            }

            services.AddSingleton(typeof(InternalModel.AppSettings.Dependencies), dependencies);

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .WriteTo.Elasticsearch(dependencies.ElasticSearch.Url)
               .CreateLogger();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            ContainerManager.RegisterServices(services, configuration,env);
            ContainerManager.RegisterExtensions(services, configuration);

            services.AddMvc().AddJsonOptions(options =>
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            );

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = dependencies.Redis.Configuration;
                options.InstanceName = dependencies.Redis.InstanceName;
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
                    context.Database.EnsureCreated();

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
