using System.ComponentModel.DataAnnotations;
using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Interviews.Dtos;

public class UpdateInterviewRequest : IValidatableObject
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [EnumDataType(typeof(InterviewType))]
    public InterviewType Type { get; set; }
    [EnumDataType(typeof(ExperienceLevel))]
    public ExperienceLevel Level { get; set; }

    public DateTime ScheduledAt { get; set; }
    [Range(15, 240)]
    public int DurationMinutes { get; set; }

    [StringLength(1000)]
    public string? Topics { get; set; }
    [StringLength(2000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ScheduledAt == default || ScheduledAt.Kind != DateTimeKind.Utc)
            yield return new ValidationResult("ScheduledAt must be a valid UTC date and time.", [nameof(ScheduledAt)]);
    }
}
