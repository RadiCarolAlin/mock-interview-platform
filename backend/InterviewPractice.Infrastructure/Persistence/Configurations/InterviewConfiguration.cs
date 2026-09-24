using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPractice.Infrastructure.Persistence.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews", table =>
        {
            table.HasCheckConstraint("CK_Interviews_Type", "\"Type\" IN (1, 2, 3)");
            table.HasCheckConstraint("CK_Interviews_Level", "\"Level\" IN (1, 2, 3, 4)");
            table.HasCheckConstraint("CK_Interviews_Status", "\"Status\" IN (1, 2, 3, 4)");
            table.HasCheckConstraint("CK_Interviews_DurationMinutes", "\"DurationMinutes\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Level)
            .IsRequired();

        builder.Property(x => x.ScheduledAt)
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.Topics)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.Candidate)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Interviewer)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.InterviewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
