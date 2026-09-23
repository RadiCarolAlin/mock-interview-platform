using InterviewPractice.Application.Auth.Dtos;

namespace InterviewPractice.Application.Auth;

public interface ICurrentUserService
{
    Task<CurrentUserDto?> GetOrLinkAsync(
        string oktaUserId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> groups,
        CancellationToken cancellationToken = default);
}