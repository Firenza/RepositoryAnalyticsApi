using Dapper.FluentMap;
using Microsoft.EntityFrameworkCore;

namespace RepositoryAnalyticsApi.Repositories.Model.EntityFramework
{
    public class RepositoryAnalysisContext : DbContext
    {
        public RepositoryAnalysisContext(DbContextOptions<RepositoryAnalysisContext> options)
        : base(options)
        {
        }

        public DbSet<RepositoryCurrentState> RepositoryCurrentState { get; set; }

        /// <summary>
        /// Any DB configuration that is not possible via EF Core.
        /// </summary>
        public void ManualConfiguration()
        {
            this.Database.ExecuteSqlCommand("CREATE EXTENSION IF NOT EXISTS pg_trgm; CREATE INDEX IF NOT EXISTS trgm_idx_repository_dependency_name ON public.repository_dependency USING gin (name gin_trgm_ops)");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure Dapper to map PostgreSql snake case column names to pascal case .NET property names
            FluentMapper.Initialize(config =>
            {
                config.AddConvention<SnakeCaseToPascalCasePropertyTransformConvention>()
                      .ForEntitiesInCurrentAssembly();
            });
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
