using InterviewPractice.Application.Reports;
using InterviewPractice.Application.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Interviewer")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ReportOverviewDto>> GetOverview(
        CancellationToken cancellationToken)
    {
        var overview = await _reportService.GetOverviewAsync(
            cancellationToken);

        return Ok(overview);
    }

    [HttpGet("candidates/{candidateId:guid}/progress")]
    public async Task<ActionResult<CandidateProgressDto>> GetCandidateProgress(
        Guid candidateId,
        CancellationToken cancellationToken)
    {
        var progress = await _reportService.GetCandidateProgressAsync(
            candidateId,
            cancellationToken);

        if (progress is null)
        {
            return NotFound();
        }

        return Ok(progress);
    }
}