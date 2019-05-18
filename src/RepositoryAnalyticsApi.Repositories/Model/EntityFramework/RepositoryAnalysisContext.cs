﻿using Dapper.FluentMap;
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
            //modelBuilder.HasDefaultSchema("repositoryAnalytics");

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
