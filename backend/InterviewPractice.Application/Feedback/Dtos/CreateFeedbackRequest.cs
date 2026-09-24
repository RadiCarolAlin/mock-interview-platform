using InterviewPractice.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;
using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Feedback.Dtos;

public class CreateFeedbackRequest
{
    [NonEmptyGuid]
    public Guid InterviewId { get; set; }

    [Range(1, 10)]
    public int OverallScore { get; set; }

    [Required, StringLength(2000)]
    public string Strengths { get; set; } = string.Empty;
    [Required, StringLength(2000)]
    public string ImprovementAreas { get; set; } = string.Empty;

    [EnumDataType(typeof(InterviewOutcome))]
    public InterviewOutcome Outcome { get; set; }

    [StringLength(3000)]
    public string? AdditionalComments { get; set; }
}
