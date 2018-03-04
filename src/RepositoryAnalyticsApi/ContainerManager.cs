using GraphQl.NetStandard.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using System.Reflection;
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

            services.AddTransient<IRepositoryManager, RepositoryManager>();
            services.AddTransient<IRepositoryRepository, MongoRepositoryRepository>();
            services.AddTransient<IDependencyRepository, MongoDependencyRepository>();
            services.AddTransient<IRepositorySourceManager, RepositorySourceManager>();
            services.AddTransient<IRepositoryAnalysisManager, RepositoryAnalysisManager>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<IRepositorySourceRepository>(serviceProvider => codeRepo);

            services.AddTransient<IEnumerable<IDependencyScraperManager>>((serviceProvider) => new List<IDependencyScraperManager> {
                new BowerDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new DotNetProjectFileDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NpmDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NuGetDependencyScraperManager(serviceProvider.GetService<IRepositorySourceManager>())
            });

            // Add in mongo dependencies
            var client = new MongoClient(mongoDbConnection);
            var db = client.GetDatabase(mongoDbDatabase);

            services.AddScoped((serviceProvider) => db);
            services.AddScoped((serviceProvider) => db.GetCollection<ServiceModel.Repository>("repository"));
        }

        public static void RegisterExtensions(IServiceCollection services, IConfiguration configuration)
        {
            // Load internal extensions
            var extensionAssemblyConfiguration = new ContainerConfiguration().WithAssembly(typeof(ExtensionAssembly).GetTypeInfo().Assembly);

            using (var extensionAssemblyContainer = extensionAssemblyConfiguration.CreateContainer())
            {
                var typeAndImplementationDerivers = extensionAssemblyContainer.GetExports<IDeriveRepositoryTypeAndImplementations>();

                foreach (var typeAndImplementationDeriver in typeAndImplementationDerivers)
                {
                    Log.Logger.Information($"Loading internalIDeriveRepositoryTypeAndImplementations {typeAndImplementationDeriver.GetType().Name}");
                }

                services.AddTransient((serviceProvider) => typeAndImplementationDerivers);
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
