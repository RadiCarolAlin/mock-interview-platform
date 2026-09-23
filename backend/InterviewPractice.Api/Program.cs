using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Application.Feedback;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Reports;
using InterviewPractice.Infrastructure.Persistence;
using InterviewPractice.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using InterviewPractice.Application.Auth;
using InterviewPractice.Api.Authorization;
using InterviewPractice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Controllers + Swagger
// -------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------
// CORS - Angular frontend
// -------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// -------------------------------------------------------
// Database
// -------------------------------------------------------

var connectionString = builder.Configuration
                           .GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException(
                           "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ApplicationDbContext>());

// -------------------------------------------------------
// Application services
// -------------------------------------------------------

builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// -------------------------------------------------------
// Okta configuration
// -------------------------------------------------------

var oktaAuthority = builder.Configuration["Okta:Authority"]
                    ?? throw new InvalidOperationException(
                        "Okta:Authority was not configured.");

var oktaClientId = builder.Configuration["Okta:ClientId"]
                   ?? throw new InvalidOperationException(
                       "Okta:ClientId was not configured.");

var oktaClientSecret = builder.Configuration["Okta:ClientSecret"]
                       ?? throw new InvalidOperationException(
                           "Okta:ClientSecret was not configured.");

// -------------------------------------------------------
// Authentication - Cookie + OpenID Connect
// -------------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        // API requests return 401 instead of automatically
        // redirecting to Okta.
        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "InterviewPractice.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    })
    .AddOpenIdConnect(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            options.Authority = oktaAuthority;
            options.ClientId = oktaClientId;
            options.ClientSecret = oktaClientSecret;

            options.ResponseType = "code";

            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath =
                "/signout-callback-oidc";

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
        });

// -------------------------------------------------------
// Authorization
// -------------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Interviewer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new UserRoleRequirement(UserRole.Interviewer));
    });

    options.AddPolicy("Candidate", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new UserRoleRequirement(UserRole.Candidate));
    });
});

builder.Services.AddScoped<
    IAuthorizationHandler,
    UserRoleAuthorizationHandler>();

// -------------------------------------------------------
// Build
// -------------------------------------------------------

var app = builder.Build();

// -------------------------------------------------------
// Development
// -------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    // Apply pending EF Core migrations automatically.
    await dbContext.Database.MigrateAsync();

    // Seed development/demo data.
    await DevelopmentDataSeeder.SeedAsync(dbContext);
}

// -------------------------------------------------------
// Middleware
// -------------------------------------------------------

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// -------------------------------------------------------
// Controllers
// -------------------------------------------------------

app.MapControllers();

app.Run();