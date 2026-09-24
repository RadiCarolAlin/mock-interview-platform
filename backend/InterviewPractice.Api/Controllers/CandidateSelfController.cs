using InterviewPractice.Application.Common.Validation;
using System.Security.Claims;
using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Candidates.Dtos;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Application.Reports;
using InterviewPractice.Application.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/candidate/me")]
[Authorize(Policy = "Candidate")]
public class CandidateSelfController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly IInterviewService _interviewService;
    private readonly IReportService _reportService;

    public CandidateSelfController(ICandidateService candidateService,
        IInterviewService interviewService,
        IReportService reportService)
    {
        _candidateService = candidateService;
        _interviewService = interviewService;
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<CandidateDetailsDto>> GetProfile(
        CancellationToken cancellationToken)
    {
        var candidateId = await GetCurrentCandidateIdAsync(
            cancellationToken);

        if (candidateId is null)
        {
            return NotFound();
        }

        var candidate = await _candidateService.GetByIdAsync(
            candidateId.Value,
            cancellationToken);

        if (candidate is null)
        {
            return NotFound();
        }

        return Ok(candidate);
    }

    [HttpGet("interviews")]
    public async Task<ActionResult<IReadOnlyList<InterviewDto>>> GetInterviews(
        CancellationToken cancellationToken)
    {
        var candidateId = await GetCurrentCandidateIdAsync(
            cancellationToken);

        if (candidateId is null)
        {
            return NotFound();
        }

        var interviews = await _interviewService.GetByCandidateIdAsync(
            candidateId.Value,
            cancellationToken);

        return Ok(interviews);
    }

    [HttpGet("progress")]
    public async Task<ActionResult<CandidateProgressDto>> GetProgress(
        CancellationToken cancellationToken)
    {
        var candidateId = await GetCurrentCandidateIdAsync(
            cancellationToken);

        if (candidateId is null)
        {
            return NotFound();
        }

        var progress = await _reportService.GetCandidateProgressAsync(
            candidateId.Value,
            cancellationToken);

        if (progress is null)
        {
            return NotFound();
        }

        return Ok(progress);
    }
    [HttpGet("interviews/{interviewId:guid}")]
    public async Task<IActionResult> GetInterview(
        [NonEmptyGuid] Guid interviewId,
        CancellationToken cancellationToken)
    {
        var candidateId = await GetCurrentCandidateIdAsync(
            cancellationToken);

        if (candidateId is null)
        {
            return NotFound();
        }

        var interview = await _interviewService.GetCandidateInterviewAsync(
            interviewId,
            candidateId.Value,
            cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        return Ok(interview);
    }
    private Task<Guid?> GetCurrentCandidateIdAsync(
        CancellationToken cancellationToken)
    {
        var oktaUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return _candidateService.GetProfileIdByOktaUserIdAsync(
            oktaUserId,
            cancellationToken);
    }
}
