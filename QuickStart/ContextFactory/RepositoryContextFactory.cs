using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace QuickStart.ContextFactory
{
    public class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext>
    {
        public RepositoryContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<RepositoryContext>()
                .UseMySql(configuration.GetConnectionString("sqlConnection"),
                new MySqlServerVersion(new Version(5, 6, 16)),
                b => b.MigrationsAssembly("QuickStart"));
            var connectionString = configuration.GetConnectionString("sqlConnection");
            Console.WriteLine($"Current Connection String: {connectionString}");

            return new RepositoryContext(builder.Options);
        }
    }
}
