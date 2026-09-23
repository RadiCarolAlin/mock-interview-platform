using InterviewPractice.Application.Auth.Dtos;
using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Application.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IApplicationDbContext _dbContext;

    public CurrentUserService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurrentUserDto?> GetOrLinkAsync(
        string oktaUserId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> groups,
        CancellationToken cancellationToken = default)
    {
        oktaUserId = oktaUserId.Trim();
        email = email.Trim().ToLowerInvariant();
        firstName = firstName.Trim();
        lastName = lastName.Trim();

        var isCandidate = groups.Contains(
            "Candidates",
            StringComparer.OrdinalIgnoreCase);

        var isInterviewer = groups.Contains(
            "Interviewers",
            StringComparer.OrdinalIgnoreCase);

        // Invalid or ambiguous role assignment.
        if (isCandidate == isInterviewer)
        {
            return null;
        }

        // Already linked user.
        var user = await _dbContext.Users
            .Include(x => x.CandidateProfile)
            .Include(x => x.InterviewerProfile)
            .FirstOrDefaultAsync(
                x => x.OktaUserId == oktaUserId,
                cancellationToken);

        if (user is not null)
        {
            return MapToDto(user);
        }

        // Existing application user on first Okta login.
        user = await _dbContext.Users
            .Include(x => x.CandidateProfile)
            .Include(x => x.InterviewerProfile)
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);

        if (user is not null)
        {
            // Do not allow the Okta group to contradict
            // the role already stored in the application.
            if (isCandidate && user.Role != UserRole.Candidate)
            {
                return null;
            }

            if (isInterviewer && user.Role != UserRole.Interviewer)
            {
                return null;
            }

            user.OktaUserId = oktaUserId;
            user.FirstName = firstName;
            user.LastName = lastName;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToDto(user);
        }

        // New user: provision from the trusted Okta group.
        user = new User
        {
            Id = Guid.NewGuid(),
            OktaUserId = oktaUserId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = isCandidate
                ? UserRole.Candidate
                : UserRole.Interviewer
        };

        if (isCandidate)
        {
            var candidateProfile = new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TargetRole = string.Empty,
                ExperienceLevel = ExperienceLevel.Junior
            };

            user.CandidateProfile = candidateProfile;

            _dbContext.Users.Add(user);
            _dbContext.CandidateProfiles.Add(candidateProfile);
        }
        else
        {
            var interviewerProfile = new InterviewerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user
            };

            user.InterviewerProfile = interviewerProfile;

            _dbContext.Users.Add(user);
            _dbContext.InterviewerProfiles.Add(interviewerProfile);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    private static CurrentUserDto MapToDto(User user)
    {
        return new CurrentUserDto
        {
            UserId = user.Id,
            OktaUserId = user.OktaUserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CandidateId = user.CandidateProfile?.Id,
            InterviewerId = user.InterviewerProfile?.Id
        };
    }
}