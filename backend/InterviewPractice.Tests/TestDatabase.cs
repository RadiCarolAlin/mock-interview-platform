using InterviewPractice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Tests;

internal static class TestDatabase
{
    // Application behavior only: this provider does not enforce PostgreSQL constraints.
    public static ApplicationDbContext Create() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
