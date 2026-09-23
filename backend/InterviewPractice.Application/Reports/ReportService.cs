using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Application.Reports.Dtos;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Application.Reports;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _dbContext;

    public ReportService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportOverviewDto> GetOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var totalCandidates = await _dbContext.CandidateProfiles
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalInterviews = await _dbContext.Interviews
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var completedInterviews = await _dbContext.Interviews
            .AsNoTracking()
            .CountAsync(
                x => x.Status == InterviewStatus.Completed,
                cancellationToken);

        var scheduledInterviews = await _dbContext.Interviews
            .AsNoTracking()
            .CountAsync(
                x => x.Status == InterviewStatus.Scheduled,
                cancellationToken);

        var averageScore = await _dbContext.Feedbacks
            .AsNoTracking()
            .Select(x => (double?)x.OverallScore)
            .AverageAsync(cancellationToken);

        return new ReportOverviewDto
        {
            TotalCandidates = totalCandidates,
            TotalInterviews = totalInterviews,
            CompletedInterviews = completedInterviews,
            ScheduledInterviews = scheduledInterviews,
            AverageScore = averageScore
        };
    }

    public async Task<CandidateProgressDto?> GetCandidateProgressAsync(
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await _dbContext.CandidateProfiles
            .AsNoTracking()
            .Where(x => x.Id == candidateId)
            .Select(x => new
            {
                x.Id,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            return null;
        }

        var interviews = await _dbContext.Interviews
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateId)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new CandidateProgressInterviewDto
            {
                InterviewId = x.Id,
                Title = x.Title,
                ScheduledAt = x.ScheduledAt,

                Score = x.Feedback != null
                    ? x.Feedback.OverallScore
                    : null,

                Outcome = x.Feedback != null
                    ? x.Feedback.Outcome
                    : null
            })
            .ToListAsync(cancellationToken);

        var completedInterviews = await _dbContext.Interviews
            .AsNoTracking()
            .CountAsync(
                x => x.CandidateId == candidateId &&
                     x.Status == InterviewStatus.Completed,
                cancellationToken);

        var scores = interviews
            .Where(x => x.Score.HasValue)
            .Select(x => x.Score!.Value)
            .ToList();

        double? averageScore = scores.Count > 0
            ? scores.Average()
            : null;

        var latestOutcome = interviews
            .Where(x => x.Outcome.HasValue)
            .Select(x => x.Outcome)
            .FirstOrDefault();

        return new CandidateProgressDto
        {
            CandidateId = candidate.Id,
            CandidateName =
                $"{candidate.FirstName} {candidate.LastName}",

            TotalInterviews = interviews.Count,
            CompletedInterviews = completedInterviews,
            AverageScore = averageScore,
            LatestOutcome = latestOutcome,
            Interviews = interviews
        };
    }
}