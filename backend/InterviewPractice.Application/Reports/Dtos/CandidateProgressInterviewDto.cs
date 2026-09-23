using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Reports.Dtos;

public class CandidateProgressInterviewDto
{
    public Guid InterviewId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    public int? Score { get; set; }

    public InterviewOutcome? Outcome { get; set; }
}