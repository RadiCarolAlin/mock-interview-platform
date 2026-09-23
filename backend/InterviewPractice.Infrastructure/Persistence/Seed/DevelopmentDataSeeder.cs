using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        const string interviewerEmail = "john.interviewer@example.com";

        // ---------------------------------------------------------
        // INTERVIEWER
        // ---------------------------------------------------------

        var interviewer = await dbContext.InterviewerProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.User.Email == interviewerEmail,
                cancellationToken);

        if (interviewer is null)
        {
            var interviewerUser = new User
            {
                Id = Guid.NewGuid(),
                OktaUserId = $"dev-{Guid.NewGuid()}",
                Email = interviewerEmail,
                FirstName = "John",
                LastName = "Interviewer",
                Role = UserRole.Interviewer
            };

            interviewer = new InterviewerProfile
            {
                Id = Guid.NewGuid(),
                UserId = interviewerUser.Id,
                User = interviewerUser
            };

            dbContext.InterviewerProfiles.Add(interviewer);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // ---------------------------------------------------------
        // DEMO DATA
        // ---------------------------------------------------------

        const string demoMarkerEmail = "alex.popescu@example.com";

        var demoDataAlreadyExists = await dbContext.Users
            .AnyAsync(
                x => x.Email == demoMarkerEmail,
                cancellationToken);

        if (demoDataAlreadyExists)
        {
            return;
        }

        // ---------------------------------------------------------
        // CANDIDATE 1 - ROXANA
        // Real demo candidate that can be linked to Okta
        // ---------------------------------------------------------

        const string roxanaEmail = "radica2020@gmail.com";

        var roxana = await dbContext.CandidateProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.User.Email == roxanaEmail,
                cancellationToken);

        if (roxana is null)
        {
            var roxanaUser = new User
            {
                Id = Guid.NewGuid(),
                OktaUserId = $"pending-{Guid.NewGuid()}",
                Email = roxanaEmail,
                FirstName = "Roxana",
                LastName = "Maria",
                Role = UserRole.Candidate
            };

            roxana = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = roxanaUser.Id,
                User = roxanaUser,
                TargetRole = "Senior .NET Developer",
                ExperienceLevel = ExperienceLevel.Senior
            };

            dbContext.CandidateProfiles.Add(roxana);
        }

        // ---------------------------------------------------------
        // CANDIDATE 2 - ALEX
        // ---------------------------------------------------------

        var alexUser = new User
        {
            Id = Guid.NewGuid(),
            OktaUserId = $"demo-{Guid.NewGuid()}",
            Email = "alex.popescu@example.com",
            FirstName = "Alex",
            LastName = "Popescu",
            Role = UserRole.Candidate
        };

        var alex = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = alexUser.Id,
            User = alexUser,
            TargetRole = "Full-Stack Developer",
            ExperienceLevel = ExperienceLevel.Mid
        };

        // ---------------------------------------------------------
        // CANDIDATE 3 - MARIA
        // ---------------------------------------------------------

        var mariaUser = new User
        {
            Id = Guid.NewGuid(),
            OktaUserId = $"demo-{Guid.NewGuid()}",
            Email = "maria.ionescu@example.com",
            FirstName = "Maria",
            LastName = "Ionescu",
            Role = UserRole.Candidate
        };

        var maria = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = mariaUser.Id,
            User = mariaUser,
            TargetRole = "Frontend Developer",
            ExperienceLevel = ExperienceLevel.Junior
        };

        // ---------------------------------------------------------
        // CANDIDATE 4 - ANDREI
        // ---------------------------------------------------------

        var andreiUser = new User
        {
            Id = Guid.NewGuid(),
            OktaUserId = $"demo-{Guid.NewGuid()}",
            Email = "andrei.marin@example.com",
            FirstName = "Andrei",
            LastName = "Marin",
            Role = UserRole.Candidate
        };

        var andrei = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = andreiUser.Id,
            User = andreiUser,
            TargetRole = "Backend Developer",
            ExperienceLevel = ExperienceLevel.Senior
        };

        // ---------------------------------------------------------
        // CANDIDATE 5 - ELENA
        // ---------------------------------------------------------

        var elenaUser = new User
        {
            Id = Guid.NewGuid(),
            OktaUserId = $"demo-{Guid.NewGuid()}",
            Email = "elena.stan@example.com",
            FirstName = "Elena",
            LastName = "Stan",
            Role = UserRole.Candidate
        };

        var elena = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = elenaUser.Id,
            User = elenaUser,
            TargetRole = "Software Engineer",
            ExperienceLevel = ExperienceLevel.Lead
        };

        dbContext.CandidateProfiles.AddRange(
            alex,
            maria,
            andrei,
            elena);

        // Save candidates first so their relationships are established.
        await dbContext.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------
        // ROXANA INTERVIEWS
        // ---------------------------------------------------------

        var roxanaInterview1 = CreateInterview(
            roxana.Id,
            interviewer.Id,
            "Senior .NET Technical Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            DateTime.UtcNow.AddDays(-20),
            60,
            "C#, .NET, Entity Framework Core, SQL",
            InterviewStatus.Completed);

        var roxanaInterview2 = CreateInterview(
            roxana.Id,
            interviewer.Id,
            "Backend Architecture Interview",
            InterviewType.SystemDesign,
            ExperienceLevel.Senior,
            DateTime.UtcNow.AddDays(-10),
            60,
            "REST APIs, Clean Architecture, scalability",
            InterviewStatus.Completed);

        var roxanaInterview3 = CreateInterview(
            roxana.Id,
            interviewer.Id,
            "Advanced .NET Mock Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            DateTime.UtcNow.AddDays(4),
            60,
            "Concurrency, performance, distributed systems",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // ALEX INTERVIEWS
        // ---------------------------------------------------------

        var alexInterview1 = CreateInterview(
            alex.Id,
            interviewer.Id,
            "Full-Stack Technical Interview",
            InterviewType.Technical,
            ExperienceLevel.Mid,
            DateTime.UtcNow.AddDays(-15),
            60,
            "Angular, TypeScript, C#, REST APIs",
            InterviewStatus.Completed);

        var alexInterview2 = CreateInterview(
            alex.Id,
            interviewer.Id,
            "System Design Practice",
            InterviewType.SystemDesign,
            ExperienceLevel.Mid,
            DateTime.UtcNow.AddDays(2),
            60,
            "API design, caching, database design",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // MARIA INTERVIEWS
        // ---------------------------------------------------------

        var mariaInterview1 = CreateInterview(
            maria.Id,
            interviewer.Id,
            "Frontend Fundamentals Interview",
            InterviewType.Technical,
            ExperienceLevel.Junior,
            DateTime.UtcNow.AddDays(-8),
            45,
            "JavaScript, TypeScript, Angular fundamentals",
            InterviewStatus.Completed);

        var mariaInterview2 = CreateInterview(
            maria.Id,
            interviewer.Id,
            "Angular Practice Interview",
            InterviewType.Technical,
            ExperienceLevel.Junior,
            DateTime.UtcNow.AddDays(6),
            45,
            "Angular components, services, RxJS",
            InterviewStatus.Scheduled);

        // ---------------------------------------------------------
        // ANDREI INTERVIEWS
        // ---------------------------------------------------------

        var andreiInterview1 = CreateInterview(
            andrei.Id,
            interviewer.Id,
            "Senior Backend Interview",
            InterviewType.Technical,
            ExperienceLevel.Senior,
            DateTime.UtcNow.AddDays(-5),
            60,
            "C#, PostgreSQL, microservices, Docker",
            InterviewStatus.Completed);

        // ---------------------------------------------------------
        // ELENA INTERVIEWS
        // ---------------------------------------------------------

        var elenaInterview1 = CreateInterview(
            elena.Id,
            interviewer.Id,
            "Lead Engineer System Design",
            InterviewType.SystemDesign,
            ExperienceLevel.Lead,
            DateTime.UtcNow.AddDays(-3),
            75,
            "Distributed systems, scalability, architecture",
            InterviewStatus.Completed);

        var elenaInterview2 = CreateInterview(
            elena.Id,
            interviewer.Id,
            "Leadership & Behavioral Interview",
            InterviewType.Behavioral,
            ExperienceLevel.Lead,
            DateTime.UtcNow.AddDays(8),
            60,
            "Leadership, mentoring, technical decisions",
            InterviewStatus.Scheduled);

        dbContext.Interviews.AddRange(
            roxanaInterview1,
            roxanaInterview2,
            roxanaInterview3,
            alexInterview1,
            alexInterview2,
            mariaInterview1,
            mariaInterview2,
            andreiInterview1,
            elenaInterview1,
            elenaInterview2);

        await dbContext.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------
        // FEEDBACK
        // ---------------------------------------------------------

        dbContext.Feedbacks.AddRange(
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
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Interview CreateInterview(
        Guid candidateId,
        Guid interviewerId,
        string title,
        InterviewType type,
        ExperienceLevel level,
        DateTime scheduledAt,
        int durationMinutes,
        string topics,
        InterviewStatus status)
    {
        return new Interview
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            InterviewerId = interviewerId,
            Title = title,
            Type = type,
            Level = level,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Topics = topics,
            Notes = "Development demo data.",
            Status = status
        };
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
            Id = Guid.NewGuid(),
            InterviewId = interviewId,
            OverallScore = overallScore,
            Strengths = strengths,
            ImprovementAreas = improvementAreas,
            Outcome = outcome,
            AdditionalComments = additionalComments
        };
    }
}