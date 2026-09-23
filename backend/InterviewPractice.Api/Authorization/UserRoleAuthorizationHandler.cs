using System.Security.Claims;
using InterviewPractice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace InterviewPractice.Api.Authorization;

public class UserRoleAuthorizationHandler
    : AuthorizationHandler<UserRoleRequirement>
{
    private readonly ApplicationDbContext _dbContext;

    public UserRoleAuthorizationHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserRoleRequirement requirement)
    {
        var oktaUserId =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(oktaUserId))
        {
            return;
        }

        var hasRole = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(x =>
                x.OktaUserId == oktaUserId &&
                x.Role == requirement.Role);

        if (hasRole)
        {
            context.Succeed(requirement);
        }
    }
}