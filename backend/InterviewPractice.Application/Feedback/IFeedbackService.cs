using InterviewPractice.Application.Feedback.Dtos;

namespace InterviewPractice.Application.Feedback;

public interface IFeedbackService
{
    Task<FeedbackDto?> GetByInterviewIdAsync(
        Guid interviewId,
        CancellationToken cancellationToken = default);

    Task<FeedbackDto> CreateAsync(
        CreateFeedbackRequest request,
        CancellationToken cancellationToken = default);
}