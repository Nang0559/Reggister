// FVN_REGISTER.Infrastructure/FVNWEBAPPContextFactory.cs
namespace FVN_REGISTER.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Chỉ dùng bởi EF Core Tools (dotnet ef migrations/database update) khi chạy từ
    /// command line — KHÔNG dùng ở runtime thật (đó là Program.cs AddDbContext).
    /// Đọc appsettings.json từ FVN_REGISTER.API để lấy connection string, không cần
    /// khởi động toàn bộ DI container của API.
    /// </summary>
    public class FVNWEBAPPContextFactory : IDesignTimeDbContextFactory<FVNWEBAPPContext>
    {
        public FVNWEBAPPContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "FVN_REGISTER.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<FVNWEBAPPContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FVNWEBAPPContext(optionsBuilder.Options);
        }
    }
}