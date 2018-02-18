using GraphQl.NetStandard.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Octokit;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnaltyicsApi.Managers.Dependencies;
using RepositoryAnalyticsApi.Extensibliity;
using RepositoryAnalyticsApi.Extensions;
using RepositoryAnalyticsApi.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition.Hosting;
using System.Reflection;

namespace RepositoryAnalyticsApi
{
    public static class ContainerManager
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Read in environment variables
            var gitHubv3ApiUrl = configuration["GITHUB_V3_API_URL"];
            var gitHubGraphQLApiUrl = configuration["GITHUB_GRAPHQL_API_URL"];
            var gitHubAccessToken = configuration["GITHUB_ACCESS_TOKEN"];

            // Setup GitHub V3 Api clients
            var gitHubV3ApiCredentials = new Credentials(gitHubAccessToken);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl));
            gitHubClient.Credentials = gitHubV3ApiCredentials;
            var gitHubTreesClient = new TreesClient(new ApiConnection(new Connection(new ProductHeaderValue("RepositoryAnalyticsApi"), new Uri(gitHubv3ApiUrl)) { Credentials = gitHubV3ApiCredentials }));

            // Setup GitHub GraphQL client
            var requestHeaders = new NameValueCollection();
            requestHeaders.Add("Authorization", $"Bearer {gitHubAccessToken}");
            requestHeaders.Add("User-Agent", "RepositoryAnalyticsApi");
            var graphQLClient = new GraphQLClient(configuration["GITHUB_GRAPHQL_API_URL"], requestHeaders);

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
            var client = new MongoClient(new MongoClientSettings
            {
                SocketTimeout = new TimeSpan(0, 0, 0, 2),
                Server = new MongoServerAddress("localhost", 27017),
                ConnectTimeout = new TimeSpan(0, 0, 0, 2),
                ServerSelectionTimeout = new TimeSpan(0, 0, 0, 2)
            });

            client = new MongoClient("mongodb://mongodb:27017");

            var db = client.GetDatabase("local");

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
                    Console.WriteLine($"Importing internal IDeriveRepositoryTypeAndImplementations for type {typeAndImplementationDeriver.TypeName} and implementation {typeAndImplementationDeriver.ImplementationName}");
                }

                services.AddTransient((serviceProvider) => typeAndImplementationDerivers);
            }
        }
    }
}
