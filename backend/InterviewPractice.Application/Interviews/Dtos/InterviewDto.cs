using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Interviews.Dtos;

public class InterviewDto
{
    public Guid Id { get; set; }

    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public InterviewType Type { get; set; }
    public ExperienceLevel Level { get; set; }

    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }

    public InterviewStatus Status { get; set; }
}