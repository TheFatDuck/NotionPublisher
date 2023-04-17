using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using RedmineApi.Data.Entities;
using RedmineApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RedmineApi.Data
{
    internal class PostgresDbContext : DbContext
    {
        //Host=localhost; Database=dotnet-6-crud-api; Username=postgres; Password=mysecretpassword
        protected readonly RedmineApiConfigs _config;
        private readonly string dbUser;
        private readonly string dbPass;
        private readonly string dbHost;
        private readonly string dbPort;
        public PostgresDbContext(RedmineApiConfigs configs)
        {
            this._config = configs;
            dbUser = _config.PgUser;
            dbPass = _config.PgPass;
            dbHost = _config.PgHost;
            dbPort = _config.PgPort;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = $"User ID={dbUser};Password={dbPass};Host={dbHost};Port={dbPort};Database=redmine;";
            optionsBuilder.UseNpgsql(connectionString);
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.np_api_key).IsUnique();

            modelBuilder.Entity<Config>()
                .HasOne(c => c.user).WithMany(u => u.configs).HasForeignKey(c=>c.user_id).IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>().HasIndex(p => p.project_name);

            modelBuilder.Entity<Issue>().HasIndex(i => i.updated_on);
            modelBuilder.Entity<Issue>().HasIndex(i => i.last_posted_on);

            modelBuilder.Entity<Page>()
                .HasOne(p => p.user).WithMany(u => u.pages).HasForeignKey(p => p.user_id).IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Page>()
                .HasOne(p => p.issue).WithMany(i => i.pages).HasForeignKey(p => p.issue_id).IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserProject>().HasKey(up => new { up.user_id, up.project_id });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
    }
}
