using InterviewPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FeedbackEntity = InterviewPractice.Domain.Entities.Feedback;

namespace InterviewPractice.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<CandidateProfile> CandidateProfiles { get; }

    DbSet<InterviewerProfile> InterviewerProfiles { get; }

    DbSet<Interview> Interviews { get; }

    DbSet<FeedbackEntity> Feedbacks { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}