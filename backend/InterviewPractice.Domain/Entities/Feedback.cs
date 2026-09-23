using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }

    public Guid InterviewId { get; set; }

    public Interview Interview { get; set; } = null!;

    public int OverallScore { get; set; }

    public string Strengths { get; set; } = string.Empty;

    public string ImprovementAreas { get; set; } = string.Empty;

    public InterviewOutcome Outcome { get; set; }

    public string? AdditionalComments { get; set; }
}