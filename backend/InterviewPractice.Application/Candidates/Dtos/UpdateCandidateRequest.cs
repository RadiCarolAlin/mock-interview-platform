using System.ComponentModel.DataAnnotations;
using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Candidates.Dtos;

public class UpdateCandidateRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(150)]
    public string TargetRole { get; set; } = string.Empty;
    [EnumDataType(typeof(ExperienceLevel))]
    public ExperienceLevel ExperienceLevel { get; set; }
}