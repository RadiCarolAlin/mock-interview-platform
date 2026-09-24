using InterviewPractice.Application.Auth;
using InterviewPractice.Domain.Entities;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InterviewPractice.Tests;

public class IdentityTests
{
    [Fact]
    public async Task Missing_and_ambiguous_role_groups_are_rejected()
    {
        using var db = TestDatabase.Create();
        var service = new CurrentUserService(db);
        foreach (var groups in new[] { Array.Empty<string>(), new[] { "Candidates", "Interviewers" } })
            Assert.Null(await service.GetOrLinkAsync("okta-id", "test@example.com", "Test", "User", groups));
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Real_identity_cannot_be_relinked_by_matching_email()
    {
        using var db = TestDatabase.Create();
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "test@example.com", OktaUserId = "original-id", Role = UserRole.Candidate });
        await db.SaveChangesAsync();
        var result = await new CurrentUserService(db).GetOrLinkAsync(
            "different-id", "test@example.com", "New", "Name", ["Interviewers"]);
        Assert.Null(result);
        db.ChangeTracker.Clear();
        var saved = await db.Users.SingleAsync();
        Assert.Equal("original-id", saved.OktaUserId);
        Assert.Equal(UserRole.Candidate, saved.Role);
        Assert.Empty(db.InterviewerProfiles);
    }

    [Fact]
    public async Task Role_changes_preserve_profiles_and_interview_history()
    {
        using var db = TestDatabase.Create();
        var service = new CurrentUserService(db);
        var candidate = await service.GetOrLinkAsync("okta-id", "test@example.com", "Test", "User", ["Candidates"]);
        Assert.NotNull(candidate);
        db.Interviews.Add(new Interview { Id = Guid.NewGuid(), CandidateId = candidate.CandidateId!.Value, Status = InterviewStatus.Completed });
        await db.SaveChangesAsync();
        var interviewer = await service.GetOrLinkAsync("okta-id", "test@example.com", "Test", "User", ["Interviewers"]);
        Assert.NotNull(interviewer);
        Assert.Equal(UserRole.Interviewer, interviewer.Role);
        Assert.Equal(candidate.CandidateId, interviewer.CandidateId);
        Assert.NotNull(interviewer.InterviewerId);
        var restored = await service.GetOrLinkAsync("okta-id", "test@example.com", "Test", "User", ["Candidates"]);
        Assert.NotNull(restored);
        Assert.Equal(UserRole.Candidate, restored.Role);
        Assert.Equal(interviewer.InterviewerId, restored.InterviewerId);
        Assert.Single(db.CandidateProfiles);
        Assert.Single(db.InterviewerProfiles);
        Assert.Single(db.Interviews);
    }
}
