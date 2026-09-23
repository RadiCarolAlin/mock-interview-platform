using InterviewPractice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace InterviewPractice.Api.Authorization;

public class UserRoleRequirement : IAuthorizationRequirement
{
    public UserRole Role { get; }

    public UserRoleRequirement(UserRole role)
    {
        Role = role;
    }
}