using Microsoft.Extensions.DependencyInjection;
using RepositoryAnaltyicsApi.Interfaces;
using RepositoryAnaltyicsApi.Managers;
using RepositoryAnalyticsApi.Repositories;

namespace RepositoryAnalyticsApi
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IRepositoryManager, RepositoryManager>();
            services.AddTransient<IRepositoryRepository, MongoRepositoryRepository>();

            return services;
        }
    }
}
