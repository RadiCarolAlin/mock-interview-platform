using InterviewPractice.Application.Common.Validation;
using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Candidates.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Interviewer")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CandidateDto>>> GetAll(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var candidates = await _candidateService.GetAllAsync(
            search,
            cancellationToken);

        return Ok(candidates);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidateDetailsDto>> GetById(
        [NonEmptyGuid] Guid id,
        CancellationToken cancellationToken)
    {
        var candidate = await _candidateService.GetByIdAsync(
            id,
            cancellationToken);

        if (candidate is null)
        {
            return NotFound();
        }

        return Ok(candidate);
    }

    [HttpPost]
    public async Task<ActionResult<CandidateDetailsDto>> Create(
        CreateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        var candidate = await _candidateService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = candidate.Id },
            candidate);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [NonEmptyGuid] Guid id,
        UpdateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _candidateService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
}