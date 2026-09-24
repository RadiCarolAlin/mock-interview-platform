using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InterviewPractice.Infrastructure.Persistence;

// Scaffold migrations without executing API startup, migrations, seeding or cloud secret access.
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Scaffolding only needs a provider. Applying a migration requires an explicit
        // ConnectionStrings__DefaultConnection environment variable or --connection.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=interview_practice_design;Username=design_only;Pooling=false";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
