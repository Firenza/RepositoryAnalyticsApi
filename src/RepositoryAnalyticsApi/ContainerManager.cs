using AutoMapper;
using AutoMapper.EquivalencyExpression;
using GraphQl.NetStandard.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnaltyicsApi.Managers.Dependencies;
using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.Extensions;
using RepositoryAnalyticsApi.Repositories;
using RepositoryAnalyticsApi.Repositories.Model.EntityFramework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition.Hosting;
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

        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Read in environment variables
            var gitHubv3ApiUrl = ReadEnvironmentVariable("GITHUB_V3_API_URL", configuration);
            var gitHubGraphQLApiUrl = ReadEnvironmentVariable("GITHUB_GRAPHQL_API_URL", configuration);
            var gitHubAccessToken = ReadEnvironmentVariable("GITHUB_ACCESS_TOKEN", configuration);
            var dbType = ReadEnvironmentVariable("DB_TYPE", configuration);
            var dbConnectionString = ReadEnvironmentVariable("DB_CONNECTION_STRING", configuration);

            // Load config data 
            var caching = configuration.GetSection("Caching").Get<InternalModel.AppSettings.Caching>();
            services.AddSingleton(typeof(InternalModel.AppSettings.Caching), caching);

            // Setup GitHub V3 Api clients
            var gitHubV3ApiCredentials = new Credentials(gitHubAccessToken);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl));
            gitHubClient.Credentials = gitHubV3ApiCredentials;
            var gitHubTreesClient = new TreesClient(new ApiConnection(new Connection(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl)) { Credentials = gitHubV3ApiCredentials }));

            // Setup GitHub GraphQL client
            var requestHeaders = new NameValueCollection();
            requestHeaders.Add("Authorization", $"Bearer {gitHubAccessToken}");
            requestHeaders.Add("User-Agent", "RepositoryAnalyticsApi");
            var graphQLClient = new GraphQLClient(new HttpClient(), gitHubGraphQLApiUrl, requestHeaders);

            IRepositorySourceRepository codeRepo = new GitHubApiRepositorySourceRepository(gitHubClient, gitHubTreesClient, graphQLClient);

            services.AddTransient<IRepositoryManager, RepositoryManager>();
            //services.AddTransient<IDependencyRepository, MongoDependencyRepository>();
            services.AddTransient<IRepositorySourceManager, RepositorySourceManager>();
            services.AddTransient<IRepositoryAnalysisManager, RepositoryAnalysisManager>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<IRepositorySourceRepository>(serviceProvider => codeRepo);
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


            var formattedDbType = dbType.ToLower().Replace(" ", string.Empty);
            if (formattedDbType == "sqlserver")
            {
                services.AddDbContext<RepositoryAnalysisContext>(options =>
                {
                    options.UseSqlServer(dbConnectionString);
                });
            }
            else if (formattedDbType == "postgresql")
            {
                services.AddDbContext<RepositoryAnalysisContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString);
                });
            }
            else
            {
                throw new ArgumentException("Unsupported DB Type, only PostgreSQL and SQL Server are supported");
            }

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

        /// <summary>
        /// Reads an environment varible based on whether or not the api is running in docker or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string ReadEnvironmentVariable(string name, IConfiguration configuration)
        {
            // If the environment variable specified in the RepositoryAnaltyicsApi startup coniguration is not there
            // then we must be running inside of Docker so just load the enviornment variables normally
            if (configuration["RUNNING_OUTSIDE_OF_DOCKER"] != "true")
            {
                return configuration[name];
            }
            else
            {
                var configurationFileLines = File.ReadAllLines("configuration.env");

                foreach (var configurationFileLine in configurationFileLines)
                {
                    var match = Regex.Match(configurationFileLine, $"^{name}=(.*)");

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }

                return null;
            }
        }
    }
}
