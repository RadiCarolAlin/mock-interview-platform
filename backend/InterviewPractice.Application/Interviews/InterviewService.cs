using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Application.Interviews;

public class InterviewService : IInterviewService
{
    private readonly IApplicationDbContext _dbContext;

    public InterviewService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InterviewDto>> GetAllAsync(
        string? search = null,
        InterviewStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Interviews
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(
                    x.Title.ToLower(),
                    $"%{searchTerm}%") ||

                EF.Functions.Like(
                    x.Candidate.User.FirstName.ToLower(),
                    $"%{searchTerm}%") ||

                EF.Functions.Like(
                    x.Candidate.User.LastName.ToLower(),
                    $"%{searchTerm}%") ||

                (x.Topics != null &&
                 EF.Functions.Like(
                     x.Topics.ToLower(),
                     $"%{searchTerm}%")));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new InterviewDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                CandidateName =
                    x.Candidate.User.FirstName + " " +
                    x.Candidate.User.LastName,
                Title = x.Title,
                Type = x.Type,
                Level = x.Level,
                ScheduledAt = x.ScheduledAt,
                DurationMinutes = x.DurationMinutes,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InterviewDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Interviews
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new InterviewDetailsDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                CandidateName =
                    x.Candidate.User.FirstName + " " +
                    x.Candidate.User.LastName,

                InterviewerId = x.InterviewerId,
                InterviewerName =
                    x.Interviewer.User.FirstName + " " +
                    x.Interviewer.User.LastName,

                Title = x.Title,
                Type = x.Type,
                Level = x.Level,
                ScheduledAt = x.ScheduledAt,
                DurationMinutes = x.DurationMinutes,
                Topics = x.Topics,
                Notes = x.Notes,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InterviewDetailsDto> CreateAsync(
        CreateInterviewRequest request,
        Guid interviewerId,
        CancellationToken cancellationToken = default)
    {
        var candidateExists = await _dbContext.CandidateProfiles
            .AnyAsync(
                x => x.Id == request.CandidateId,
                cancellationToken);

        if (!candidateExists)
        {
            throw new InvalidOperationException(
                "Candidate does not exist.");
        }

        var interviewerExists = await _dbContext.InterviewerProfiles
            .AnyAsync(
                x => x.Id == interviewerId,
                cancellationToken);

        if (!interviewerExists)
        {
            throw new InvalidOperationException(
                "Interviewer does not exist.");
        }

        var interview = new Interview
        {
            Id = Guid.NewGuid(),
            CandidateId = request.CandidateId,
            InterviewerId = interviewerId,
            Title = request.Title.Trim(),
            Type = request.Type,
            Level = request.Level,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            Topics = request.Topics?.Trim(),
            Notes = request.Notes?.Trim(),
            Status = InterviewStatus.Scheduled
        };

        _dbContext.Interviews.Add(interview);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(
            interview.Id,
            cancellationToken))!;
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateInterviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var interview = await _dbContext.Interviews
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (interview is null)
        {
            return false;
        }

        if (interview.Status == InterviewStatus.Completed)
        {
            throw new InvalidOperationException(
                "A completed interview cannot be modified.");
        }

        interview.Title = request.Title.Trim();
        interview.Type = request.Type;
        interview.Level = request.Level;
        interview.ScheduledAt = request.ScheduledAt;
        interview.DurationMinutes = request.DurationMinutes;
        interview.Topics = request.Topics?.Trim();
        interview.Notes = request.Notes?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var interview = await _dbContext.Interviews
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (interview is null)
        {
            return false;
        }

        if (interview.Status == InterviewStatus.Completed)
        {
            return true;
        }

        interview.Status = InterviewStatus.Completed;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<InterviewDto>> GetByCandidateIdAsync(
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Interviews
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateId)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new InterviewDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                CandidateName =
                    x.Candidate.User.FirstName + " " +
                    x.Candidate.User.LastName,
                Title = x.Title,
                Type = x.Type,
                Level = x.Level,
                ScheduledAt = x.ScheduledAt,
                DurationMinutes = x.DurationMinutes,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }
}