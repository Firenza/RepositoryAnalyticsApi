using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryAnalysisContext : DbContext
    {
        public RepositoryAnalysisContext(DbContextOptions<RepositoryAnalysisContext> options)
        : base(options)
        {
            //this.ConfigureLogging(s => Debug.WriteLine(s));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Snake case everything to avoid having to put quotes around everything
                // when using PostgreSQL

                // Replace table names
                entityType.Relational().TableName = entityType.Relational().TableName.ToSnakeCase();

                // Replace column names            
                foreach (var property in entityType.GetProperties())
                {
                    property.Relational().ColumnName = property.Name.ToSnakeCase();
                }

                foreach (var key in entityType.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var key in entityType.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var index in entityType.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name.ToSnakeCase();
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
