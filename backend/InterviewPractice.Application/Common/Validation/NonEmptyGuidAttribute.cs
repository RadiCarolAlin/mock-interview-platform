using System.ComponentModel.DataAnnotations;

namespace InterviewPractice.Application.Common.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NonEmptyGuidAttribute : ValidationAttribute
{
    public NonEmptyGuidAttribute() : base("{0} must be a non-empty identifier.") { }

    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
}
