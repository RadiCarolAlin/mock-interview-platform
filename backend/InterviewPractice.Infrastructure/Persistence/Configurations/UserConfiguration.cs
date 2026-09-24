using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPractice.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table =>
        {
            table.HasCheckConstraint("CK_Users_Role", "\"Role\" IN (1, 2)");
            // Explicit .NET whitespace characters; independent of database locale.
            table.HasCheckConstraint("CK_Users_OktaUserId_NotBlank",
                "btrim(\"OktaUserId\", U&'\\0009\\000A\\000B\\000C\\000D\\0020\\0085\\00A0\\1680\\2000\\2001\\2002\\2003\\2004\\2005\\2006\\2007\\2008\\2009\\200A\\2028\\2029\\202F\\205F\\3000') <> ''");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OktaUserId)
            .IsConcurrencyToken()
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.OktaUserId)
            .IsUnique();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .IsRequired();
    }
}
