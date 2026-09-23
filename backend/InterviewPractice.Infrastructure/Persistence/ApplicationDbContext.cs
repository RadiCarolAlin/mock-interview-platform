using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<InterviewerProfile> InterviewerProfiles => Set<InterviewerProfile>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}