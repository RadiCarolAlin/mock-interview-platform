using System.Security.Claims;
using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Candidates.Dtos;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Application.Reports;
using InterviewPractice.Application.Reports.Dtos;
using InterviewPractice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/candidate/me")]
[Authorize(Policy = "Candidate")]
public class CandidateSelfController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICandidateService _candidateService;
    private readonly IInterviewService _interviewService;
    private readonly IReportService _reportService;

    public CandidateSelfController(
        ApplicationDbContext dbContext,
        ICandidateService candidateService,
        IInterviewService interviewService,
        IReportService reportService)
    {
        _dbContext = dbContext;
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
        Guid interviewId,
        CancellationToken cancellationToken)
    {
        var candidateId = await GetCurrentCandidateIdAsync(
            cancellationToken);

        if (candidateId is null)
        {
            return NotFound();
        }

        var interview = await _dbContext.Interviews
            .AsNoTracking()
            .Where(x =>
                x.Id == interviewId &&
                x.CandidateId == candidateId.Value)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,

                interviewer =
                    x.Interviewer.User.FirstName + " " +
                    x.Interviewer.User.LastName,

                type = x.Type,
                level = x.Level,
                scheduledAt = x.ScheduledAt,
                durationMinutes = x.DurationMinutes,
                topics = x.Topics,
                notes = x.Notes,
                status = x.Status,

                feedback = x.Feedback == null
                    ? null
                    : new
                    {
                        overallScore = x.Feedback.OverallScore,
                        outcome = x.Feedback.Outcome,
                        strengths = x.Feedback.Strengths,
                        improvementAreas =
                            x.Feedback.ImprovementAreas,
                        additionalComments =
                            x.Feedback.AdditionalComments
                    }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        return Ok(interview);
    }
    private async Task<Guid?> GetCurrentCandidateIdAsync(
        CancellationToken cancellationToken)
    {
        var oktaUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(oktaUserId))
        {
            return null;
        }

        return await _dbContext.CandidateProfiles
            .AsNoTracking()
            .Where(x => x.User.OktaUserId == oktaUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}