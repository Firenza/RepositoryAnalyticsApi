using AutoMapper;
using AutoMapper.EquivalencyExpression;
using GraphQl.NetStandard.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnaltyicsApi.Managers.Dependencies;
using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.Extensions;
using RepositoryAnalyticsApi.InternalModel.AppSettings;
using RepositoryAnalyticsApi.Repositories;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition.Hosting;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace RepositoryAnalyticsApi
{
    public static class ContainerManager
    {

        public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostingEnvironment env, Dependencies dependencies, Caching caching)
        {
            // Put caching config in container 
            //var caching = configuration.GetSection("Caching").Get<InternalModel.AppSettings.Caching>();
            services.AddSingleton(typeof(InternalModel.AppSettings.Caching), caching);

            // Load config data
            //var dependencies = configuration.GetSection("Dependencies").Get<InternalModel.AppSettings.Dependencies>();
            services.AddSingleton(typeof(InternalModel.AppSettings.Dependencies), dependencies);

            // ---------------------------------
            // -- Configure GitHub API access --
            // ---------------------------------

            // Read in the github api token value (either from local secrets or from environment variables)
            var gitHubTokenSecretName = $"GithubAccessToken";
            var gitHubAccessToken = configuration[gitHubTokenSecretName];

            if (string.IsNullOrWhiteSpace(gitHubAccessToken))
            {
                throw new ArgumentException($"No GitHub API token variable named '{gitHubTokenSecretName}' found in local secrets or enviornment variables");
            }

            // Setup GitHub V3 Api clients
            var gitHubV3ApiCredentials = new Credentials(gitHubAccessToken);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(dependencies.GitHub.V3ApiUrl));
            gitHubClient.Credentials = gitHubV3ApiCredentials;
            var gitHubTreesClient = new TreesClient(new ApiConnection(new Connection(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(dependencies.GitHub.V3ApiUrl)) { Credentials = gitHubV3ApiCredentials }));

            // Setup GitHub GraphQL client
            var requestHeaders = new NameValueCollection();
            requestHeaders.Add("Authorization", $"Bearer {gitHubAccessToken}");
            requestHeaders.Add("User-Agent", "RepositoryAnalyticsApi");
            var graphQLClient = new GraphQLClient(new HttpClient(), dependencies.GitHub.GraphQlApiUrl, requestHeaders);

            // ----------------------------
            // -- Configure the Database --
            // ----------------------------

            var dbConnectionStringBuilder = new DbConnectionStringBuilder();
            dbConnectionStringBuilder.ConnectionString = dependencies.Database.ConnectionString;

            // It's possible to use things like windows auth when running outside of a container so check for the User Id
            // value in the connection string before trying to look for a local secret pwd
            if (dbConnectionStringBuilder.ConnectionString.Contains("User Id", StringComparison.OrdinalIgnoreCase))
            {
                Log.Logger.Debug("User Id found in connection string, attempting to integrate password into connection string");

                var dbPasswordSecretName = $"DatabasePassword";
                var dbPassword = configuration[dbPasswordSecretName];

                if (string.IsNullOrWhiteSpace(dbPassword))
                {
                    throw new ArgumentException($"No DB password token variable named '{dbPasswordSecretName}' found in local secrets or enviornment variables");
                }
                else
                {
                    Log.Logger.Debug("Password found in configuration, adding it to connection string");
                }

                dbConnectionStringBuilder["Password"] = dbPassword;
            }
            else
            {
                Log.Logger.Debug("No User Id found in connection string, assuming passowrd less (E.G. Windows Auth) authentication is in use");
            }

            var connestionString = dbConnectionStringBuilder.ConnectionString;

            // Figure out which DB type needs to be loaded
            var formattedDbType = dependencies.Database.Type.ToLower().Replace(" ", string.Empty);

            if (formattedDbType == "sqlserver")
            {
                services.AddDbContext<RepositoryAnalysisContext>(options =>
                {
                    options.UseSqlServer(connestionString);
                });
            }
            else if (formattedDbType == "postgresql")
            {
                services.AddDbContext<RepositoryAnalysisContext>(options =>
                {
                    options.UseNpgsql(connestionString);
                });
            }
            else
            {
                throw new ArgumentException("Unsupported DB Type, only PostgreSQL and SQL Server are supported");
            }

            // Now setup all the mapping between the Entity Framework objects
            var config = new MapperConfiguration(cfg =>
            {
                var versionManager = new VersionManager();

                cfg.AddCollectionMappers();

                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryCurrentState, ServiceModel.RepositoryDevOpsIntegrations>();
                cfg.CreateMap<ServiceModel.RepositoryDevOpsIntegrations, Repositories.Model.EntityFramework.RepositoryCurrentState>();
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryCurrentState, ServiceModel.RepositoryCurrentState>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.RepositoryId))
                .ForMember(dest => dest.DevOpsIntegrations, opt => opt.MapFrom(source => source));
                cfg.CreateMap<ServiceModel.RepositoryCurrentState, Repositories.Model.EntityFramework.RepositoryCurrentState>()
                .ForMember(dest => dest.RepositoryId, opt => opt.MapFrom(source => source.Id))
                .ForMember(dest => dest.ContinuousDelivery, opt => opt.MapFrom(source => source.DevOpsIntegrations.ContinuousDelivery))
                .ForMember(dest => dest.ContinuousIntegration, opt => opt.MapFrom(source => source.DevOpsIntegrations.ContinuousIntegration))
                .ForMember(dest => dest.ContinuousDeployment, opt => opt.MapFrom(source => source.DevOpsIntegrations.ContinuousDeployment));
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryDependency, ServiceModel.RepositoryDependency>();
                cfg.CreateMap<ServiceModel.RepositoryDependency, Repositories.Model.EntityFramework.RepositoryDependency>()
                    .ForMember(dest => dest.PaddedVersion, opt => opt.MapFrom(source => versionManager.GetPaddedVersion(source.Version)))
                    .ForMember(dest => dest.MinorVerison, opt => opt.MapFrom(source => versionManager.GetMinorVersion(source.Version)))
                    .ForMember(dest => dest.PreReleaseSemanticVersion, opt => opt.MapFrom(source => versionManager.GetPreReleaseVersion(source.Version)))
                    .EqualityComparison((source, dest) => $"{source.Name}|{source.Version}|{source.RepoPath}" == $"{dest.Name}|{dest.Version}|{dest.RepoPath}");
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryFile, ServiceModel.RepositoryFile>();
                cfg.CreateMap<ServiceModel.RepositoryFile, Repositories.Model.EntityFramework.RepositoryFile>()
                    .EqualityComparison((source, dest) => source.FullPath == dest.FullPath);
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryImplementation, ServiceModel.RepositoryImplementation>();
                cfg.CreateMap<ServiceModel.RepositoryImplementation, Repositories.Model.EntityFramework.RepositoryImplementation>()
                    .EqualityComparison((source, dest) => source.Name == dest.Name);
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryTeam, ServiceModel.RepositoryTeam>();
                cfg.CreateMap<ServiceModel.RepositoryTeam, Repositories.Model.EntityFramework.RepositoryTeam>()
                    .EqualityComparison((source, dest) => source.Name == dest.Name);
                cfg.CreateMap<Repositories.Model.EntityFramework.Topic, ServiceModel.RepositoryTopic>();
                cfg.CreateMap<ServiceModel.RepositoryTopic, Repositories.Model.EntityFramework.Topic>()
                    .EqualityComparison((source, dest) => source.Name == dest.Name);
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositoryTypeAndImplementations, ServiceModel.RepositoryTypeAndImplementations>();
                cfg.CreateMap<ServiceModel.RepositoryTypeAndImplementations, Repositories.Model.EntityFramework.RepositoryTypeAndImplementations>()
                    .EqualityComparison((source, dest) => source.TypeName == dest.TypeName);
                cfg.CreateMap<Repositories.Model.EntityFramework.RepositorySnapshot, ServiceModel.RepositorySnapshot>();
                cfg.CreateMap<ServiceModel.RepositorySnapshot, Repositories.Model.EntityFramework.RepositorySnapshot>()
                    .EqualityComparison((source, dest) => source.WindowStartCommitId == dest.WindowStartCommitId);
            });

            services.AddSingleton(config.CreateMapper());

            // -------------------------
            // Configure DI container --
            // -------------------------

            services.AddTransient<IRepositoryManager, RepositoryManager>();
            services.AddTransient<IDependencyRepository, RelationalDependencyRepository>();
            services.AddTransient<IRepositorySourceManager, RepositorySourceManager>();
            services.AddTransient<IRepositoryAnalysisManager, RepositoryAnalysisManager>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<IRepositorySourceRepository>(serviceProvider => new GitHubApiRepositorySourceRepository(
                   gitHubClient,
                   gitHubTreesClient,
                   graphQLClient,
                   serviceProvider.GetService<ILogger<GitHubApiRepositorySourceRepository>>()
            ));
            services.AddTransient<IRepositoryImplementationsManager, RepositoryImplementationsManager>();
            //services.AddTransient<IRepositoryImplementationsRepository, MongoRepositoryImplementationsRepository>();
            services.AddTransient<IRepositoriesTypeNamesManager, RepositoriesTypeNamesManager>();
            //services.AddTransient<IRepositoriesTypeNamesRepository, MongoRepositoriesTypeNamesRepository>();
            //services.AddTransient<IRepositorySearchRepository, MongoRepositorySearchRepository>();
            services.AddTransient<IVersionManager, VersionManager>();
            services.AddTransient<IRepositoryCurrentStateRepository, RelationalRepositoryCurrentStateRepository>();
            services.AddTransient<IRepositorySnapshotRepository, RelationalRepositorySnapshotRepository>();

            services.AddTransient<IEnumerable<IDependencyScraperManager>>((serviceProvider) => new List<IDependencyScraperManager> {
                new BowerDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new DotNetProjectFileDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NpmDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NuGetDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>())
            });
        }

        public static void RegisterExtensions(IServiceCollection services, IConfiguration configuration)
        {
            var typeAndImplementationDerivers = new List<IDeriveRepositoryTypeAndImplementations>();
            IDeriveRepositoryDevOpsIntegrations devOpsIntegrationDeriver = null;

            var internalExtensionAssembly = typeof(ExtensionAssembly).GetTypeInfo().Assembly;

            // Load internal extensions
            var extensionAssemblyConfiguration = new ContainerConfiguration().WithAssembly(internalExtensionAssembly);

            using (var extensionAssemblyContainer = extensionAssemblyConfiguration.CreateContainer())
            {
                var internalTypeAndImplementationDerivers = extensionAssemblyContainer.GetExports<IDeriveRepositoryTypeAndImplementations>();

                foreach (var typeAndImplementationDeriver in internalTypeAndImplementationDerivers)
                {
                    Log.Logger.Information($"Loading internal IDeriveRepositoryTypeAndImplementations {typeAndImplementationDeriver.GetType().Name}");
                }

                typeAndImplementationDerivers.AddRange(internalTypeAndImplementationDerivers);

            }

            // Load external extensions
            List<Assembly> externalPluginDirectoryAssemblies = null;

            try
            {
                // For some reason when the MEF container is created it needs help resolving dependency references
                AssemblyLoadContext.Default.Resolving += ResolveAssemblyDependency;

                var externalPluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

                Log.Logger.Information($"Scanning directory {externalPluginDirectory} for external plugins");

                // Load any external extensions in the defined plugin directory
                externalPluginDirectoryAssemblies = LoadAssembliesFromDirectory(externalPluginDirectory);

                if (!externalPluginDirectoryAssemblies.Any())
                {
                    Log.Logger.Information($"No external plugins found");
                }
                else
                {
                    var externalAssemblyConfiguration = new ContainerConfiguration().WithAssemblies(externalPluginDirectoryAssemblies);

                    using (var externalAssemblyContainer = externalAssemblyConfiguration.CreateContainer())
                    {
                        var loadedExternalTypeAndImplementationDerivers = externalAssemblyContainer.GetExports<IDeriveRepositoryTypeAndImplementations>();

                        foreach (var externalTypeAndImplementationDeriver in loadedExternalTypeAndImplementationDerivers)
                        {
                            Log.Logger.Information($"Loading external IDeriveRepositoryTypeAndImplementations {externalTypeAndImplementationDeriver.GetType().Name}");
                        }

                        typeAndImplementationDerivers.AddRange(loadedExternalTypeAndImplementationDerivers);

                        if (externalAssemblyContainer.TryGetExport<IDeriveRepositoryDevOpsIntegrations>(out var loadedExternalDevOpsImplementationDerivers))
                        {
                            Log.Logger.Information($"Loading external IDeriveRepositoryDevOpsIntegrations {loadedExternalDevOpsImplementationDerivers.GetType().Name}");

                            devOpsIntegrationDeriver = loadedExternalDevOpsImplementationDerivers;
                        }
                    }
                }

            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Logger.Error(ex, "!!!! Error when loading external plugin assemblies !!!!!\n");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Log.Logger.Error(loaderException, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "!!!! Error when loading external plugin assemblies !!!!!\n");
            }

            // Now add any lists of extension types that we found in the container
            services.AddTransient((serviceProvider) => typeAndImplementationDerivers.AsEnumerable());

            if (devOpsIntegrationDeriver != null)
            {
                services.AddTransient((serviceProvider) => devOpsIntegrationDeriver);
            }
            else
            {
                Log.Logger.Information("No external IDeriveRepositoryDevOpsIntegrations found, loading NoOp implementation");

                services.AddTransient<IDeriveRepositoryDevOpsIntegrations>((serviceProvider) => new NoOpDevOpsIntegrationDeriver());
            }


            Assembly ResolveAssemblyDependency(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
            {
                var matchingAssembly = externalPluginDirectoryAssemblies.FirstOrDefault(assembly => assembly.FullName == assemblyName.FullName);

                return matchingAssembly;
            }

            List<Assembly> LoadAssembliesFromDirectory(string directory)
            {
                var assemblies = new List<Assembly>();

                if (Directory.Exists(directory))
                {
                    foreach (var file in Directory.GetFiles(directory, "*.dll"))
                    {
                        try
                        {
                            var assembly = Assembly.LoadFile(file);
                            assemblies.Add(assembly);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                return assemblies;
            }
        }
    }
}
