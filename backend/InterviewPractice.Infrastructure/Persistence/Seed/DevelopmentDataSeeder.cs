using System.Security.Cryptography;
using System.Text;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    private const string DemoNotes = "Development demo data.";

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        // Serialize this seeder across processes; the lock is released on commit/rollback.
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(7483926102451)", cancellationToken);

        var seedTime = DateTime.UtcNow;
        var interviewerUser = await EnsureUserAsync("john", "john.interviewer@example.com",
            "John", "Interviewer", UserRole.Interviewer, "dev");
        var interviewer = interviewerUser.InterviewerProfile;
        if (interviewer is null)
        {
            interviewer = new InterviewerProfile
            {
                Id = StableId("interviewer/john"),
                UserId = interviewerUser.Id,
                User = interviewerUser
            };
            interviewerUser.InterviewerProfile = interviewer;
            dbContext.InterviewerProfiles.Add(interviewer);
        }

        var roxana = await EnsureCandidateAsync("roxana", "radica2020@gmail.com",
            "Roxana", "Maria", "Senior .NET Developer", ExperienceLevel.Senior, "pending");
        var alex = await EnsureCandidateAsync("alex", "alex.popescu@example.com",
            "Alex", "Popescu", "Full-Stack Developer", ExperienceLevel.Mid);
        var maria = await EnsureCandidateAsync("maria", "maria.ionescu@example.com",
            "Maria", "Ionescu", "Frontend Developer", ExperienceLevel.Junior);
        var andrei = await EnsureCandidateAsync("andrei", "andrei.marin@example.com",
            "Andrei", "Marin", "Backend Developer", ExperienceLevel.Senior);
        var elena = await EnsureCandidateAsync("elena", "elena.stan@example.com",
            "Elena", "Stan", "Software Engineer", ExperienceLevel.Lead);

        // ---------------------------------------------------------
        // ROXANA INTERVIEWS
        // ---------------------------------------------------------

        var roxanaInterview1 = await EnsureInterviewAsync("roxanaInterview1",
            roxana.Id,
            interviewer.Id,
            "Senior .NET Technical Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            seedTime.AddDays(-20),
            60,
            "C#, .NET, Entity Framework Core, SQL",
            InterviewStatus.Completed);

        var roxanaInterview2 = await EnsureInterviewAsync("roxanaInterview2",
            roxana.Id,
            interviewer.Id,
            "Backend Architecture Interview",
            InterviewType.SystemDesign,
            ExperienceLevel.Senior,
            seedTime.AddDays(-10),
            60,
            "REST APIs, Clean Architecture, scalability",
            InterviewStatus.Completed);

        var roxanaInterview3 = await EnsureInterviewAsync("roxanaInterview3",
            roxana.Id,
            interviewer.Id,
            "Advanced .NET Mock Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            seedTime.AddDays(4),
            60,
            "Concurrency, performance, distributed systems",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // ALEX INTERVIEWS
        // ---------------------------------------------------------

        var alexInterview1 = await EnsureInterviewAsync("alexInterview1",
            alex.Id,
            interviewer.Id,
            "Full-Stack Technical Interview",
            InterviewType.Technical,
            ExperienceLevel.Mid,
            seedTime.AddDays(-15),
            60,
            "Angular, TypeScript, C#, REST APIs",
            InterviewStatus.Completed);

        var alexInterview2 = await EnsureInterviewAsync("alexInterview2",
            alex.Id,
            interviewer.Id,
            "System Design Practice",
            InterviewType.SystemDesign,
            ExperienceLevel.Mid,
            seedTime.AddDays(2),
            60,
            "API design, caching, database design",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // MARIA INTERVIEWS
        // ---------------------------------------------------------

        var mariaInterview1 = await EnsureInterviewAsync("mariaInterview1",
            maria.Id,
            interviewer.Id,
            "Frontend Fundamentals Interview",
            InterviewType.Technical,
            ExperienceLevel.Junior,
            seedTime.AddDays(-8),
            45,
            "JavaScript, TypeScript, Angular fundamentals",
            InterviewStatus.Completed);

        var mariaInterview2 = await EnsureInterviewAsync("mariaInterview2",
            maria.Id,
            interviewer.Id,
            "Angular Practice Interview",
            InterviewType.Technical,
            ExperienceLevel.Junior,
            seedTime.AddDays(6),
            45,
            "Angular components, services, RxJS",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // ANDREI INTERVIEWS
        // ---------------------------------------------------------

        var andreiInterview1 = await EnsureInterviewAsync("andreiInterview1",
            andrei.Id,
            interviewer.Id,
            "Senior Backend Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            seedTime.AddDays(-5),
            60,
            "C#, PostgreSQL, microservices, Docker",
            InterviewStatus.Completed);

        // ---------------------------------------------------------
        // ELENA INTERVIEWS
        // ---------------------------------------------------------

        var elenaInterview1 = await EnsureInterviewAsync("elenaInterview1",
            elena.Id,
            interviewer.Id,
            "Lead Engineer System Design",
            InterviewType.SystemDesign,
            ExperienceLevel.Lead,
            seedTime.AddDays(-3),
            75,
            "Distributed systems, scalability, architecture",
            InterviewStatus.Completed);

        var elenaInterview2 = await EnsureInterviewAsync("elenaInterview2",
            elena.Id,
            interviewer.Id,
            "Leadership & Behavioral Interview",
            InterviewType.Behavioral,
            ExperienceLevel.Lead,
            seedTime.AddDays(8),
            60,
            "Leadership, mentoring, technical decisions",
            InterviewStatus.Scheduled);

        var feedbacks = new[]
        {
            CreateFeedback(
                roxanaInterview1.Id,
                8,
                "Strong .NET fundamentals and good knowledge of Entity Framework Core.",
                "Could provide more detailed explanations around concurrency and performance.",
                InterviewOutcome.Ready,
                "Good overall technical performance."),

            CreateFeedback(
                roxanaInterview2.Id,
                9,
                "Excellent understanding of backend architecture and clear communication.",
                "Could discuss more trade-offs between different caching strategies.",
                InterviewOutcome.StrongPerformance,
                "Strong improvement compared with the previous session."),

            CreateFeedback(
                alexInterview1.Id,
                7,
                "Good understanding of Angular and REST API integration.",
                "Needs more practice with advanced RxJS and backend architecture.",
                InterviewOutcome.MakingProgress,
                "Solid mid-level performance."),

            CreateFeedback(
                mariaInterview1.Id,
                6,
                "Good understanding of basic Angular concepts and TypeScript.",
                "Needs more practice with RxJS, state management and component architecture.",
                InterviewOutcome.NeedsMorePractice,
                "Good foundation for a junior developer."),

            CreateFeedback(
                andreiInterview1.Id,
                8,
                "Strong backend fundamentals and good database knowledge.",
                "Could improve explanations around distributed system failure scenarios.",
                InterviewOutcome.Ready,
                "Good senior-level technical performance."),

            CreateFeedback(
                elenaInterview1.Id,
                10,
                "Excellent system design, scalability knowledge and architectural reasoning.",
                "Could provide additional examples of cost-related architectural trade-offs.",
                InterviewOutcome.StrongPerformance,
                "Excellent lead-level performance.")
        };

        foreach (var feedback in feedbacks)
        {
            var interview = dbContext.Interviews.Local.Single(x => x.Id == feedback.InterviewId);
            // Preserve existing feedback and respect a seed interview whose status was edited.
            if (interview.Status == InterviewStatus.Completed &&
                !await dbContext.Feedbacks.AnyAsync(x => x.InterviewId == feedback.InterviewId, cancellationToken))
            {
                dbContext.Feedbacks.Add(feedback);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        async Task<User> EnsureUserAsync(string key, string email, string firstName,
            string lastName, UserRole role, string placeholderPrefix)
        {
            email = email.Trim().ToLowerInvariant();
            var id = StableId($"user/{key}");
            var matches = await dbContext.Users
                .Include(x => x.CandidateProfile)
                .Include(x => x.InterviewerProfile)
                .Where(x => x.Id == id || x.Email == email)
                .ToListAsync(cancellationToken);

            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Conflicting demo user identity for seed key '{key}'.");
            }

            // Never change an existing user's identity, role, email, names or profiles.
            if (matches.Count == 1)
            {
                return matches[0];
            }

            var user = new User
            {
                Id = id,
                OktaUserId = $"{placeholderPrefix}-{id:D}",
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role
            };
            dbContext.Users.Add(user);
            return user;
        }

        async Task<CandidateProfile> EnsureCandidateAsync(string key, string email,
            string firstName, string lastName, string targetRole, ExperienceLevel level,
            string placeholderPrefix = "demo")
        {
            var user = await EnsureUserAsync(key, email, firstName, lastName,
                UserRole.Candidate, placeholderPrefix);
            if (user.CandidateProfile is not null)
            {
                return user.CandidateProfile;
            }

            var profile = new CandidateProfile
            {
                Id = StableId($"candidate/{key}"),
                UserId = user.Id,
                User = user,
                TargetRole = targetRole,
                ExperienceLevel = level
            };
            user.CandidateProfile = profile;
            dbContext.CandidateProfiles.Add(profile);
            return profile;
        }

        async Task<Interview> EnsureInterviewAsync(string key, Guid candidateId,
            Guid interviewerId, string title, InterviewType type, ExperienceLevel level,
            DateTime scheduledAt, int durationMinutes, string topics, InterviewStatus status)
        {
            var id = StableId($"interview/{key}");
            // Recognize records created by the old random-ID seeder using its explicit
            // demo marker, participants and title. Do not adopt ordinary interviews.
            var matches = await dbContext.Interviews
                .Where(x => x.Id == id ||
                    (x.CandidateId == candidateId && x.InterviewerId == interviewerId &&
                     x.Title == title && x.Notes == DemoNotes))
                .ToListAsync(cancellationToken);

            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Ambiguous demo interview for seed key '{key}'.");
            }

            if (matches.Count == 1)
            {
                var existing = matches[0];
                if (existing.CandidateId != candidateId || existing.InterviewerId != interviewerId)
                {
                    throw new InvalidOperationException($"Conflicting demo interview participants for seed key '{key}'.");
                }
                return existing;
            }

            var interview = new Interview
            {
                Id = id,
                CandidateId = candidateId,
                InterviewerId = interviewerId,
                Title = title,
                Type = type,
                Level = level,
                ScheduledAt = scheduledAt,
                DurationMinutes = durationMinutes,
                Topics = topics,
                Notes = DemoNotes,
                Status = status
            };
            dbContext.Interviews.Add(interview);
            return interview;
        }
    }

    private static Guid StableId(string key)
    {
        // Fixed namespace and keys keep seed identities stable across machines and runs.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"InterviewPractice.DevelopmentData/{key}"));
        return new Guid(hash.AsSpan(0, 16));
    }

    private static Feedback CreateFeedback(
        Guid interviewId,
        int overallScore,
        string strengths,
        string improvementAreas,
        InterviewOutcome outcome,
        string additionalComments)
    {
        return new Feedback
        {
            Id = StableId($"feedback/{interviewId:D}"),
            InterviewId = interviewId,
            OverallScore = overallScore,
            Strengths = strengths,
            ImprovementAreas = improvementAreas,
            Outcome = outcome,
            AdditionalComments = additionalComments
        };
    }
}