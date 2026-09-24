using InterviewPractice.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InterviewPractice.Api.ErrorHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            ResourceNotFoundException => (404, exception.Message),
            BusinessConflictException => (409, exception.Message),
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" } postgres }
                when UniqueConflict(postgres.ConstraintName) is { } message => (409, message),
            _ => (500, "Something went wrong. Please try again.")
        };

        if (status == 500)
            logger.LogError(exception, "Unhandled request failure. TraceId: {TraceId}", context.TraceIdentifier);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status switch { 404 => "Not Found", 409 => "Conflict", _ => "Internal Server Error" },
            Detail = detail,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }

    private static string? UniqueConflict(string? constraint) => constraint switch
    {
        "IX_Users_Email" => "A user with this email already exists.",
        "IX_Feedbacks_InterviewId" => "Feedback already exists for this interview.",
        "IX_Users_OktaUserId" or "IX_CandidateProfiles_UserId" or "IX_InterviewerProfiles_UserId"
            => "The account was updated by another request. Please try again.",
        _ => null
    };
}
