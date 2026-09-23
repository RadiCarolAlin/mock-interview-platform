using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPractice.Infrastructure.Persistence.Configurations;

public class InterviewerProfileConfiguration : IEntityTypeConfiguration<InterviewerProfile>
{
    public void Configure(EntityTypeBuilder<InterviewerProfile> builder)
    {
        builder.ToTable("InterviewerProfiles");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithOne(x => x.InterviewerProfile)
            .HasForeignKey<InterviewerProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}