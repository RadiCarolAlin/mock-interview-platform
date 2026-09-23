using System.Security.Claims;
using InterviewPractice.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPractice.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public AuthController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "http://localhost:4200"
        };

        return Challenge(
            properties,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "http://localhost:4200"
        };

        return SignOut(
            properties,
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var oktaUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var email =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("preferred_username");

        var firstName =
            User.FindFirstValue(ClaimTypes.GivenName)
            ?? User.FindFirstValue("given_name")
            ?? string.Empty;

        var lastName =
            User.FindFirstValue(ClaimTypes.Surname)
            ?? User.FindFirstValue("family_name")
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(oktaUserId) ||
            string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new
            {
                message =
                    "Required identity claims were not provided by Okta."
            });
        }

        var groups = User.FindAll("groups")
            .Select(x => x.Value)
            .ToArray();

        var currentUser = await _currentUserService.GetOrLinkAsync(
            oktaUserId,
            email,
            firstName,
            lastName,
            groups,
            cancellationToken);

        if (currentUser is null)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "User does not have a valid application role."
                });
        }

        return Ok(new
        {
            isAuthenticated = true,
            userId = currentUser.UserId,
            oktaUserId = currentUser.OktaUserId,
            email = currentUser.Email,
            firstName = currentUser.FirstName,
            lastName = currentUser.LastName,
            role = currentUser.Role.ToString(),
            candidateId = currentUser.CandidateId,
            interviewerId = currentUser.InterviewerId
        });
    }
}