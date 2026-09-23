using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPractice.Infrastructure.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OverallScore)
            .IsRequired();

        builder.Property(x => x.Strengths)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.ImprovementAreas)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Outcome)
            .IsRequired();

        builder.Property(x => x.AdditionalComments)
            .HasMaxLength(3000);

        builder.HasOne(x => x.Interview)
            .WithOne(x => x.Feedback)
            .HasForeignKey<Feedback>(x => x.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.InterviewId)
            .IsUnique();
    }
}