using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Domain.Enums;

namespace InterviewPractice.Application.Interviews;

public interface IInterviewService
{
    Task<CandidateInterviewDetailsDto?> GetCandidateInterviewAsync(
        Guid interviewId,
        Guid candidateId,
        CancellationToken cancellationToken = default);

    Task<InterviewDetailsDto?> CreateForUserAsync(
        CreateInterviewRequest request,
        string oktaUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InterviewDto>> GetAllAsync(
        string? search = null,
        InterviewStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InterviewDto>> GetByCandidateIdAsync(
        Guid candidateId,
        CancellationToken cancellationToken = default);

    Task<InterviewDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InterviewDetailsDto> CreateAsync(
        CreateInterviewRequest request,
        Guid interviewerId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid id,
        UpdateInterviewRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
