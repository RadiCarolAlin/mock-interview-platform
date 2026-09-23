using System.Security.Claims;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Domain.Enums;
using InterviewPractice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Interviewer")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly ApplicationDbContext _dbContext;

    public InterviewsController(
        IInterviewService interviewService,
        ApplicationDbContext dbContext)
    {
        _interviewService = interviewService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InterviewDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] InterviewStatus? status,
        CancellationToken cancellationToken)
    {
        var interviews = await _interviewService.GetAllAsync(
            search,
            status,
            cancellationToken);

        return Ok(interviews);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InterviewDetailsDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var interview = await _interviewService.GetByIdAsync(
            id,
            cancellationToken);

        if (interview is null)
        {
            return NotFound();
        }

        return Ok(interview);
    }

    [HttpPost]
    public async Task<ActionResult<InterviewDetailsDto>> Create(
        CreateInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var oktaUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(oktaUserId))
        {
            return Unauthorized();
        }

        var interviewerId = await _dbContext.InterviewerProfiles
            .AsNoTracking()
            .Where(x => x.User.OktaUserId == oktaUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (interviewerId is null)
        {
            return Forbid();
        }

        var interview = await _interviewService.CreateAsync(
            request,
            interviewerId.Value,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = interview.Id },
            interview);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _interviewService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var completed = await _interviewService.CompleteAsync(
            id,
            cancellationToken);

        if (!completed)
        {
            return NotFound();
        }

        return NoContent();
    }
}