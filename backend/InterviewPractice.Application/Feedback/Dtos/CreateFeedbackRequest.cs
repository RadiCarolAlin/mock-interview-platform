using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Feedback.Dtos;

public class CreateFeedbackRequest
{
    public Guid InterviewId { get; set; }

    public int OverallScore { get; set; }

    public string Strengths { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;

    public InterviewOutcome Outcome { get; set; }

    public string? AdditionalComments { get; set; }
}