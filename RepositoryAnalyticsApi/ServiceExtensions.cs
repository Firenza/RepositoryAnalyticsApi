using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnaltyicsApi.Managers.Dependencies;
using RepositoryAnalyticsApi.Repositories;
using RepositoryAnalyticsApi.ServiceModel;
using System;
using System.Collections.Generic;

namespace RepositoryAnalyticsApi
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IRepositoryManager, RepositoryManager>();
            services.AddTransient<IRepositoryRepository, MongoRepositoryRepository>();
            services.AddTransient<IRepositorySourceManager, RepositorySourceManager>();

            services.AddTransient<IEnumerable<IDependencyManager>>((serviceProvider) => new List<IDependencyManager> {
                new BowerDependencyManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new DotNetProjectFileDependencyManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NpmDependencyManager(serviceProvider.GetService<IRepositorySourceManager>()),
                new NuGetDependencyManager(serviceProvider.GetService<IRepositorySourceManager>())
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

            services.AddScoped((serviceProvider) => db.GetCollection<Repository>("repository"));

            return services;
        }
    }
}
