using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPractice.Infrastructure.Persistence.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.ToTable("CandidateProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetRole)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ExperienceLevel)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.CandidateProfile)
            .HasForeignKey<CandidateProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}