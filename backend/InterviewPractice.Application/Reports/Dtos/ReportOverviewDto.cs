namespace InterviewPractice.Application.Reports.Dtos;

public class ReportOverviewDto
{
    public int TotalCandidates { get; set; }

    public int TotalInterviews { get; set; }

    public int CompletedInterviews { get; set; }

    public int ScheduledInterviews { get; set; }

    public double? AverageScore { get; set; }
}