using InterviewPractice.Application.Reports.Dtos;

namespace InterviewPractice.Application.Reports;

public interface IReportService
{
    Task<ReportOverviewDto> GetOverviewAsync(
        CancellationToken cancellationToken = default);

    Task<CandidateProgressDto?> GetCandidateProgressAsync(
        Guid candidateId,
        CancellationToken cancellationToken = default);
}