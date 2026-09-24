using InterviewPractice.Application.Common.Validation;
using InterviewPractice.Application.Feedback;
using InterviewPractice.Application.Feedback.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Interviewer")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet("interview/{interviewId:guid}")]
    public async Task<ActionResult<FeedbackDto>> GetByInterviewId(
        [NonEmptyGuid] Guid interviewId,
        CancellationToken cancellationToken)
    {
        var feedback = await _feedbackService.GetByInterviewIdAsync(
            interviewId,
            cancellationToken);

        if (feedback is null)
        {
            return NotFound();
        }

        return Ok(feedback);
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackDto>> Create(
        CreateFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var feedback = await _feedbackService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetByInterviewId),
            new { interviewId = feedback.InterviewId },
            feedback);
    }
}