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
        if (string.IsNullOrWhiteSpace(oktaUserId) ||
            string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

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

        var role = isCandidate ? UserRole.Candidate : UserRole.Interviewer;

        // Identity takes precedence over email, including after an email change.
        var user = await _dbContext.Users
            .Include(x => x.CandidateProfile)
            .Include(x => x.InterviewerProfile)
            .FirstOrDefaultAsync(
                x => x.OktaUserId == oktaUserId,
                cancellationToken);

        if (user is null)
        {
            // Email can only link identities explicitly provisioned as placeholders.
            user = await _dbContext.Users
                .Include(x => x.CandidateProfile)
                .Include(x => x.InterviewerProfile)
                .FirstOrDefaultAsync(
                    x => x.Email == email,
                    cancellationToken);

            if (user is not null)
            {
                if (!IsUnlinkedIdentity(user.OktaUserId))
                {
                    return null;
                }

                user.OktaUserId = oktaUserId;
                user.FirstName = firstName;
                user.LastName = lastName;
            }
            else
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    OktaUserId = oktaUserId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role
                };

                _dbContext.Users.Add(user);
            }
        }

        user.Role = role;

        // Keep both profiles and their history when the active role changes.
        if (isCandidate && user.CandidateProfile is null)
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

            _dbContext.CandidateProfiles.Add(candidateProfile);
        }
        else if (isInterviewer && user.InterviewerProfile is null)
        {
            var interviewerProfile = new InterviewerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user
            };

            user.InterviewerProfile = interviewerProfile;

            _dbContext.InterviewerProfiles.Add(interviewerProfile);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request linked the placeholder first. Never retry by email.
            return null;
        }

        return MapToDto(user);
    }

    private static bool IsUnlinkedIdentity(string identity)
    {
        foreach (var prefix in new[] { "pending-", "dev-", "demo-" })
        {
            if (identity.StartsWith(prefix, StringComparison.Ordinal) &&
                Guid.TryParseExact(identity[prefix.Length..], "D", out _))
            {
                return true;
            }
        }

        return false;
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
