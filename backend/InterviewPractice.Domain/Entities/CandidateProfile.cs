using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Domain.Entities;

public class CandidateProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string TargetRole { get; set; } = string.Empty;

    public ExperienceLevel ExperienceLevel { get; set; }

    public ICollection<Interview> Interviews { get; set; } = [];
}