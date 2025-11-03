using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Repositories.SqlContext
{
    public class SqlDbContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        public SqlDbContext CreateDbContext(string[] args)
        {
           var startupProjectPath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(startupProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("Sql");

            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); 
            });

            var interceptorLogger = loggerFactory.CreateLogger<EfExceptionLoggingInterceptor>();
            var interceptor = new EfExceptionLoggingInterceptor(interceptorLogger);

            optionsBuilder.AddInterceptors(interceptor);

            return new SqlDbContext(optionsBuilder.Options);
        }
    }
}
