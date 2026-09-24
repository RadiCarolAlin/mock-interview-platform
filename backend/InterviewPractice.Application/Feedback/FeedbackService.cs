using InterviewPractice.Application.Common.Exceptions;
using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Application.Feedback.Dtos;
using InterviewPractice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FeedbackEntity = InterviewPractice.Domain.Entities.Feedback;

namespace InterviewPractice.Application.Feedback;

public class FeedbackService : IFeedbackService
{
    private readonly IApplicationDbContext _dbContext;

    public FeedbackService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeedbackDto?> GetByInterviewIdAsync(
        Guid interviewId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Feedbacks
            .AsNoTracking()
            .Where(x => x.InterviewId == interviewId)
            .Select(x => new FeedbackDto
            {
                Id = x.Id,
                InterviewId = x.InterviewId,
                OverallScore = x.OverallScore,
                Strengths = x.Strengths,
                ImprovementAreas = x.ImprovementAreas,
                Outcome = x.Outcome,
                AdditionalComments = x.AdditionalComments
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FeedbackDto> CreateAsync(
        CreateFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var interview = await _dbContext.Interviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.InterviewId,
                cancellationToken);

        if (interview is null)
        {
            throw new ResourceNotFoundException(
                "Interview does not exist.");
        }

        if (interview.Status != InterviewStatus.Completed)
        {
            throw new BusinessConflictException(
                "Feedback can only be added to a completed interview.");
        }

        var feedbackExists = await _dbContext.Feedbacks
            .AnyAsync(
                x => x.InterviewId == request.InterviewId,
                cancellationToken);

        if (feedbackExists)
        {
            throw new BusinessConflictException(
                "Feedback already exists for this interview.");
        }

        var feedback = new FeedbackEntity
        {
            Id = Guid.NewGuid(),
            InterviewId = request.InterviewId,
            OverallScore = request.OverallScore,
            Strengths = request.Strengths.Trim(),
            ImprovementAreas = request.ImprovementAreas.Trim(),
            Outcome = request.Outcome,
            AdditionalComments = request.AdditionalComments?.Trim()
        };

        _dbContext.Feedbacks.Add(feedback);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FeedbackDto
        {
            Id = feedback.Id,
            InterviewId = feedback.InterviewId,
            OverallScore = feedback.OverallScore,
            Strengths = feedback.Strengths,
            ImprovementAreas = feedback.ImprovementAreas,
            Outcome = feedback.Outcome,
            AdditionalComments = feedback.AdditionalComments
        };
    }
}