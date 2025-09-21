using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartGate.Infrastructure.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SmartGateDbContext>
{
    public SmartGateDbContext CreateDbContext(string[] args)
    {
        // Use env var SMARTGATE_PG if provided, else default local dev string.
        var cs = Environment.GetEnvironmentVariable("SMARTGATE_PG")
                 ?? "Host=localhost;Port=15432;Database=smartgate;Username=<username>;Password=<password>;"; // Should always come secrets manager

        var options = new DbContextOptionsBuilder<SmartGateDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new SmartGateDbContext(options);
    }
}