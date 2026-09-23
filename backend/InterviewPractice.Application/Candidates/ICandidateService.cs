using InterviewPractice.Application.Candidates.Dtos;

namespace InterviewPractice.Application.Candidates;

public interface ICandidateService
{
    Task<IReadOnlyList<CandidateDto>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<CandidateDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CandidateDetailsDto> CreateAsync(
        CreateCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid id,
        UpdateCandidateRequest request,
        CancellationToken cancellationToken = default);
}