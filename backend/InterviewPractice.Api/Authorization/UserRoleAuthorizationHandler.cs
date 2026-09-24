using System.Security.Claims;
using InterviewPractice.Application.Auth;
using Microsoft.AspNetCore.Authorization;

namespace InterviewPractice.Api.Authorization;

public class UserRoleAuthorizationHandler
    : AuthorizationHandler<UserRoleRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public UserRoleAuthorizationHandler(
        ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserRoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var oktaUserId =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var email = context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("preferred_username");

        if (string.IsNullOrWhiteSpace(oktaUserId) ||
            string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var currentUser = await _currentUserService.GetOrLinkAsync(
            oktaUserId,
            email,
            context.User.FindFirstValue(ClaimTypes.GivenName)
                ?? context.User.FindFirstValue("given_name") ?? string.Empty,
            context.User.FindFirstValue(ClaimTypes.Surname)
                ?? context.User.FindFirstValue("family_name") ?? string.Empty,
            context.User.FindAll("groups").Select(x => x.Value).ToArray(),
            context.Resource is HttpContext httpContext
                ? httpContext.RequestAborted
                : CancellationToken.None);

        if (currentUser?.Role == requirement.Role)
        {
            context.Succeed(requirement);
        }
    }
}
