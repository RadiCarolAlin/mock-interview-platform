using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string OktaUserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }

    public InterviewerProfile? InterviewerProfile { get; set; }
}