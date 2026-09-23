using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Candidates.Dtos;

public class CandidateDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string TargetRole { get; set; } = string.Empty;
    public ExperienceLevel ExperienceLevel { get; set; }
}