using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Reports.Dtos;

public class CandidateProgressDto
{
    public Guid CandidateId { get; set; }

    public string CandidateName { get; set; } = string.Empty;

    public int TotalInterviews { get; set; }

    public int CompletedInterviews { get; set; }

    public double? AverageScore { get; set; }

    public InterviewOutcome? LatestOutcome { get; set; }

    public IReadOnlyList<CandidateProgressInterviewDto> Interviews { get; set; }
        = Array.Empty<CandidateProgressInterviewDto>();
}