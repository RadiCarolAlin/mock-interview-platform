using InterviewPractice.Application.Candidates.Dtos;
using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Application.Candidates;

public class CandidateService : ICandidateService
{
    private readonly IApplicationDbContext _dbContext;

    public CandidateService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CandidateDto>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CandidateProfiles
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(
                    x.User.FirstName.ToLower(),
                    $"%{searchTerm}%") ||

                EF.Functions.Like(
                    x.User.LastName.ToLower(),
                    $"%{searchTerm}%") ||

                EF.Functions.Like(
                    x.User.Email.ToLower(),
                    $"%{searchTerm}%") ||

                EF.Functions.Like(
                    x.TargetRole.ToLower(),
                    $"%{searchTerm}%"));
        }

        return await query
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.FirstName)
            .Select(x => new CandidateDto
            {
                Id = x.Id,
                Email = x.User.Email,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                TargetRole = x.TargetRole,
                ExperienceLevel = x.ExperienceLevel
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CandidateDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CandidateProfiles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CandidateDetailsDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Email = x.User.Email,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                TargetRole = x.TargetRole,
                ExperienceLevel = x.ExperienceLevel
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CandidateDetailsDto> CreateAsync(
        CreateCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AnyAsync(
                x => x.Email == email,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            // Temporary until Okta provisioning is implemented.
            OktaUserId = $"pending-{Guid.NewGuid()}",

            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.Candidate
        };

        var candidate = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            TargetRole = request.TargetRole.Trim(),
            ExperienceLevel = request.ExperienceLevel
        };

        _dbContext.CandidateProfiles.Add(candidate);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CandidateDetailsDto
        {
            Id = candidate.Id,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TargetRole = candidate.TargetRole,
            ExperienceLevel = candidate.ExperienceLevel
        };
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var candidate = await _dbContext.CandidateProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (candidate is null)
        {
            return false;
        }

        candidate.User.FirstName = request.FirstName.Trim();
        candidate.User.LastName = request.LastName.Trim();
        candidate.TargetRole = request.TargetRole.Trim();
        candidate.ExperienceLevel = request.ExperienceLevel;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}