using GraphQl.NetStandard.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Octokit;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnaltyicsApi.Managers.Dependencies;
using RepositoryAnalyticsApi.Extensibility;
using RepositoryAnalyticsApi.Extensions;
using RepositoryAnalyticsApi.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
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
            var mongoDbConnection = ReadEnvironmentVariable("MONGO_DB_CONNECTION", configuration);
            var mongoDbDatabase = ReadEnvironmentVariable("MONGO_DB_DATABASE", configuration);

            // Setup GitHub V3 Api clients
            var gitHubV3ApiCredentials = new Credentials(gitHubAccessToken);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl));
            gitHubClient.Credentials = gitHubV3ApiCredentials;
            var gitHubTreesClient = new TreesClient(new ApiConnection(new Connection(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl)) { Credentials = gitHubV3ApiCredentials }));

            // Setup GitHub GraphQL client
            var requestHeaders = new NameValueCollection();
            requestHeaders.Add("Authorization", $"Bearer {gitHubAccessToken}");
            requestHeaders.Add("User-Agent", "RepositoryAnalyticsApi");
            var graphQLClient = new GraphQLClient(gitHubGraphQLApiUrl, requestHeaders);

            IRepositorySourceRepository codeRepo = new GitHubApiRepositorySourceRepository(gitHubClient, gitHubTreesClient, graphQLClient);

            services.AddTransient<IRepositoriesManager, RepositoryManager>();
            services.AddTransient<IRepositoriesRepository, MongoRepositoryRepository>();
            services.AddTransient<IDependencyRepository, MongoDependencyRepository>();
            services.AddTransient<IRepositorySourceManager, RepositorySourceManager>();
            services.AddTransient<IRepositoryAnalysisManager, RepositoryAnalysisManager>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<IRepositorySourceRepository>(serviceProvider => codeRepo);
            services.AddTransient<IRepositoryImplementationsManager, RepositoryImplementationsManager>();
            services.AddTransient<IRepositoryImplementationsRepository, MongoRepositoryImplementationsRepository>();
            services.AddTransient<IRepositoriesTypeNamesManager, RepositoriesTypeNamesManager>();
            services.AddTransient<IRepositoriesTypeNamesRepository, MongoRepositoriesTypeNamesRepository>();

            services.AddTransient<IEnumerable<IDependencyScraperManager>>((serviceProvider) => new List<IDependencyScraperManager> {
                new BowerDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new DotNetProjectFileDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NpmDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NuGetDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>())
            });

            var mongoClientSettings = new MongoClientSettings();
            mongoClientSettings.Server = new MongoServerAddress("localhost", 27017);
            mongoClientSettings.ConnectTimeout = new TimeSpan(0, 0, 0, 2, 0);

            // Add in mongo dependencies
            var client = new MongoClient(mongoClientSettings);
            var db = client.GetDatabase(mongoDbDatabase);

            services.AddScoped((serviceProvider) => db);
            services.AddScoped((serviceProvider) => db.GetCollection<ServiceModel.Repository>("repository"));
            services.AddScoped((serviceProvider) => db.GetCollection<BsonDocument>("repository"));
        }

        public static void RegisterExtensions(IServiceCollection services, IConfiguration configuration)
        {
            var typeAndImplementationDerivers = new List<IDeriveRepositoryTypeAndImplementations>();

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

                // Load any external extensions in the defined plugin directory
                var externalPluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
                externalPluginDirectoryAssemblies = LoadAssembliesFromDirectory(externalPluginDirectory);

                Log.Logger.Information($"Scanning directory {externalPluginDirectory} for external plugins");

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

                        services.AddTransient<IDeriveRepositoryDevOpsIntegrations>(serviceProvider => loadedExternalDevOpsImplementationDerivers);
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

            Assembly ResolveAssemblyDependency(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
            {
                var matchingAssembly = externalPluginDirectoryAssemblies.FirstOrDefault(assembly => assembly.FullName == assemblyName.FullName);

                return matchingAssembly;
            }

            List<Assembly> LoadAssembliesFromDirectory(string directory)
            {
                var assemblies = new List<Assembly>();

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
