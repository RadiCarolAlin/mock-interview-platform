using System.ComponentModel.DataAnnotations;
using InterviewPractice.Application.Common.Validation;
using System.Security.Claims;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Interviews.Dtos;
using InterviewPractice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Interviewer")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewsController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InterviewDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery, EnumDataType(typeof(InterviewStatus))] InterviewStatus? status,
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
        [NonEmptyGuid] Guid id,
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

        var interview = await _interviewService.CreateForUserAsync(
            request,
            oktaUserId,
            cancellationToken);

        if (interview is null)
        {
            return Forbid();
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = interview.Id },
            interview);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [NonEmptyGuid] Guid id,
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
        [NonEmptyGuid] Guid id,
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
