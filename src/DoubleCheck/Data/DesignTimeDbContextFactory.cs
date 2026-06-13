using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DoubleCheck.Data;

/// <summary>Lets `dotnet ef migrations` run without booting the web host.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=doublecheck;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
