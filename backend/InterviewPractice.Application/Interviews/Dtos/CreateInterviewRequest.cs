using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Interviews.Dtos;

public class CreateInterviewRequest
{
    public Guid CandidateId { get; set; }

    public string Title { get; set; } = string.Empty;

    public InterviewType Type { get; set; }
    public ExperienceLevel Level { get; set; }

    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }

    public string? Topics { get; set; }
    public string? Notes { get; set; }
}