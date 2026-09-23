using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Domain.Entities;

public class Interview
{
    public Guid Id { get; set; }

    public Guid CandidateId { get; set; }

    public CandidateProfile Candidate { get; set; } = null!;

    public Guid InterviewerId { get; set; }

    public InterviewerProfile Interviewer { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public InterviewType Type { get; set; }

    public ExperienceLevel Level { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? Topics { get; set; }

    public string? Notes { get; set; }

    public InterviewStatus Status { get; set; }

    public Feedback? Feedback { get; set; }
}