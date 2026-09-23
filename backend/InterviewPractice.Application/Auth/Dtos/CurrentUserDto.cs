using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Auth.Dtos;

public class CurrentUserDto
{
    public Guid UserId { get; set; }

    public string OktaUserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public Guid? CandidateId { get; set; }

    public Guid? InterviewerId { get; set; }
}