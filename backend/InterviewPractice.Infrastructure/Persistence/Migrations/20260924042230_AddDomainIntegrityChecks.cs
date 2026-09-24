using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewPractice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainIntegrityChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_OktaUserId_NotBlank",
                table: "Users",
                sql: "btrim(\"OktaUserId\", U&'\\0009\\000A\\000B\\000C\\000D\\0020\\0085\\00A0\\1680\\2000\\2001\\2002\\2003\\2004\\2005\\2006\\2007\\2008\\2009\\200A\\2028\\2029\\202F\\205F\\3000') <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role",
                table: "Users",
                sql: "\"Role\" IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interviews_DurationMinutes",
                table: "Interviews",
                sql: "\"DurationMinutes\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interviews_Level",
                table: "Interviews",
                sql: "\"Level\" IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interviews_Status",
                table: "Interviews",
                sql: "\"Status\" IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interviews_Type",
                table: "Interviews",
                sql: "\"Type\" IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Feedbacks_Outcome",
                table: "Feedbacks",
                sql: "\"Outcome\" IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Feedbacks_OverallScore",
                table: "Feedbacks",
                sql: "\"OverallScore\" BETWEEN 1 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateProfiles_ExperienceLevel",
                table: "CandidateProfiles",
                sql: "\"ExperienceLevel\" IN (1, 2, 3, 4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_OktaUserId_NotBlank",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Interviews_DurationMinutes",
                table: "Interviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Interviews_Level",
                table: "Interviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Interviews_Status",
                table: "Interviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Interviews_Type",
                table: "Interviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Feedbacks_Outcome",
                table: "Feedbacks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Feedbacks_OverallScore",
                table: "Feedbacks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateProfiles_ExperienceLevel",
                table: "CandidateProfiles");
        }
    }
}
