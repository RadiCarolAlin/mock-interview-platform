using System.ComponentModel.DataAnnotations;
using InterviewPractice.Application.Candidates.Dtos;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Application.Feedback.Dtos;
using InterviewPractice.Domain.Enums;
using Xunit;

namespace InterviewPractice.Tests;

public class RequestValidationTests
{
    private static List<ValidationResult> Errors(object request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, true);
        return errors;
    }

    [Fact]
    public void Candidate_names_reject_empty_and_whitespace()
    {
        foreach (var name in new[] { "", "   " })
        {
            var errors = Errors(new CreateCandidateRequest
            {
                FirstName = name, LastName = "Candidate", Email = "candidate@example.com",
                ExperienceLevel = ExperienceLevel.Junior
            });
            Assert.Contains(errors, e => e.MemberNames.Contains("FirstName"));
        }
    }

    [Theory]
    [InlineData(14)]
    [InlineData(241)]
    public void Interview_duration_rejects_values_outside_limits(int duration)
    {
        var request = new UpdateInterviewRequest
        {
            Title = "Interview", Type = InterviewType.Technical, Level = ExperienceLevel.Junior,
            ScheduledAt = DateTime.UtcNow, DurationMinutes = duration
        };
        Assert.Contains(Errors(request), e => e.MemberNames.Contains("DurationMinutes"));
        request.DurationMinutes = duration < 15 ? 15 : 240;
        Assert.Empty(Errors(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Feedback_score_rejects_values_outside_limits(int score)
    {
        var request = new CreateFeedbackRequest
        {
            InterviewId = Guid.NewGuid(), OverallScore = score, Strengths = "Clear explanations",
            ImprovementAreas = "Practice SQL", Outcome = InterviewOutcome.Ready
        };
        Assert.Contains(Errors(request), e => e.MemberNames.Contains("OverallScore"));
        request.OverallScore = score < 1 ? 1 : 10;
        Assert.Empty(Errors(request));
    }

    [Fact]
    public void Undefined_experience_level_is_rejected()
    {
        var request = new UpdateCandidateRequest
        { FirstName = "Test", LastName = "Candidate", ExperienceLevel = (ExperienceLevel)99 };
        Assert.Contains(Errors(request), e => e.MemberNames.Contains("ExperienceLevel"));
    }
}
