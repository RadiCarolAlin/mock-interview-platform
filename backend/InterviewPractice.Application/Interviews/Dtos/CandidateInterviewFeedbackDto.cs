using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Interviews.Dtos;

public class CandidateInterviewFeedbackDto
{
    public int OverallScore { get; set; }
    public InterviewOutcome Outcome { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
    public string? AdditionalComments { get; set; }
}
