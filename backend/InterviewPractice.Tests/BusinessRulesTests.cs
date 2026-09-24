using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Candidates.Dtos;
using InterviewPractice.Application.Common.Exceptions;
using InterviewPractice.Application.Feedback;
using InterviewPractice.Application.Feedback.Dtos;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InterviewPractice.Tests;

public class BusinessRulesTests
{
    [Fact]
    public async Task Cancelled_interview_cannot_be_completed()
    {
        using var db = TestDatabase.Create();
        var interview = new Interview { Id = Guid.NewGuid(), Status = InterviewStatus.Cancelled };
        db.Interviews.Add(interview);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessConflictException>(() => new InterviewService(db).CompleteAsync(interview.Id));
        db.ChangeTracker.Clear();
        Assert.Equal(InterviewStatus.Cancelled, (await db.Interviews.SingleAsync()).Status);
    }

    [Fact]
    public async Task Scheduled_interview_can_be_updated_and_completed()
    {
        using var db = TestDatabase.Create();
        var interview = new Interview { Id = Guid.NewGuid(), Status = InterviewStatus.Scheduled };
        db.Interviews.Add(interview);
        await db.SaveChangesAsync();
        var service = new InterviewService(db);
        Assert.True(await service.UpdateAsync(interview.Id, new UpdateInterviewRequest
        {
            Title = " Updated title ", Type = InterviewType.Technical, Level = ExperienceLevel.Junior,
            ScheduledAt = DateTime.UtcNow, DurationMinutes = 60
        }));
        Assert.True(await service.CompleteAsync(interview.Id));
        db.ChangeTracker.Clear();
        var saved = await db.Interviews.SingleAsync();
        Assert.Equal("Updated title", saved.Title);
        Assert.Equal(60, saved.DurationMinutes);
        Assert.Equal(InterviewStatus.Completed, saved.Status);
    }

    [Fact]
    public async Task Duplicate_feedback_is_a_business_conflict()
    {
        using var db = TestDatabase.Create();
        var interview = new Interview { Id = Guid.NewGuid(), Status = InterviewStatus.Completed };
        db.Interviews.Add(interview);
        db.Feedbacks.Add(new Feedback { Id = Guid.NewGuid(), InterviewId = interview.Id });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessConflictException>(() => new FeedbackService(db)
            .CreateAsync(new CreateFeedbackRequest { InterviewId = interview.Id }));
        Assert.Equal(1, await db.Feedbacks.CountAsync());
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_after_normalization()
    {
        using var db = TestDatabase.Create();
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "test@example.com", OktaUserId = "real-okta-id" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessConflictException>(() => new CandidateService(db)
            .CreateAsync(new CreateCandidateRequest { Email = " TEST@EXAMPLE.COM " }));
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Empty(db.CandidateProfiles);
    }

    [Fact]
    public async Task Feedback_for_missing_interview_is_not_found()
    {
        using var db = TestDatabase.Create();
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => new FeedbackService(db)
            .CreateAsync(new CreateFeedbackRequest { InterviewId = Guid.NewGuid() }));
        Assert.Empty(db.Feedbacks);
    }

    [Fact]
    public async Task Interview_for_missing_candidate_is_not_found()
    {
        using var db = TestDatabase.Create();
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => new InterviewService(db)
            .CreateAsync(new CreateInterviewRequest { CandidateId = Guid.NewGuid() }, Guid.NewGuid()));
        Assert.Empty(db.Interviews);
    }
}
