using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Candidates.Dtos;

public class UpdateCandidateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string TargetRole { get; set; } = string.Empty;
    public ExperienceLevel ExperienceLevel { get; set; }
}