using InterviewPractice.Api.ErrorHandling;
using Google.Cloud.SecretManager.V1;
using InterviewPractice.Api.Authorization;
using InterviewPractice.Application.Auth;
using InterviewPractice.Application.Candidates;
using InterviewPractice.Application.Common.Interfaces;
using InterviewPractice.Application.Feedback;
using InterviewPractice.Application.Interviews;
using InterviewPractice.Application.Reports;
using InterviewPractice.Domain.Enums;
using InterviewPractice.Infrastructure.Persistence;
using InterviewPractice.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Controllers + Swagger
// -------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------
// Secret provider
// -------------------------------------------------------

var secretProvider =
    builder.Configuration["Secrets:Provider"] ?? "Environment";

string connectionString;
string oktaClientSecret;

if (secretProvider.Equals(
        "GoogleSecretManager",
        StringComparison.OrdinalIgnoreCase))
{
    var googleProjectId =
        builder.Configuration["GoogleCloud:ProjectId"]
        ?? throw new InvalidOperationException(
            "GoogleCloud:ProjectId was not configured.");

    var secretManagerClient =
        await SecretManagerServiceClient.CreateAsync();

    async Task<string> GetSecretAsync(string secretId)
    {
        var secretVersionName = new SecretVersionName(
            googleProjectId,
            secretId,
            "latest");

        var response =
            await secretManagerClient.AccessSecretVersionAsync(
                secretVersionName);

        return response.Payload.Data.ToStringUtf8().Trim();
    }

    var dbPassword = await GetSecretAsync("db-password");

    oktaClientSecret =
        await GetSecretAsync("okta-client-secret");

    var dbHost =
        builder.Configuration["Database:Host"] ?? "127.0.0.1";

    var dbPort =
        builder.Configuration["Database:Port"] ?? "5432";

    var dbName =
        builder.Configuration["Database:Name"]
        ?? throw new InvalidOperationException(
            "Database:Name was not configured.");

    var dbUser =
        builder.Configuration["Database:User"]
        ?? throw new InvalidOperationException(
            "Database:User was not configured.");

    connectionString =
        $"Host={dbHost};" +
        $"Port={dbPort};" +
        $"Database={dbName};" +
        $"Username={dbUser};" +
        $"Password={dbPassword}";
}
else
{
    connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not found.");

    oktaClientSecret =
        builder.Configuration["Okta:ClientSecret"]
        ?? throw new InvalidOperationException(
            "Okta:ClientSecret was not configured.");
}

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(
    provider =>
        provider.GetRequiredService<ApplicationDbContext>());

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

var oktaAuthority =
    builder.Configuration["Okta:Authority"]
    ?? throw new InvalidOperationException(
        "Okta:Authority was not configured.");

var oktaClientId =
    builder.Configuration["Okta:ClientId"]
    ?? throw new InvalidOperationException(
        "Okta:ClientId was not configured.");

// -------------------------------------------------------
// Authentication - Cookie + OpenID Connect
// -------------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "InterviewPractice.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;
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
// Forwarded headers
// Required when HTTPS terminates at the GKE Ingress.
// -------------------------------------------------------

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// -------------------------------------------------------
// Build
// -------------------------------------------------------

var app = builder.Build();

app.UseExceptionHandler();

// -------------------------------------------------------
// Swagger - development only
// -------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------------------------------
// Database migrations + optional demo seed
// -------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    // Apply EF Core migrations in both local and cloud environments.
    await dbContext.Database.MigrateAsync();

    var seedDemoData =
        builder.Configuration.GetValue<bool>("SeedDemoData");

    if (app.Environment.IsDevelopment() && seedDemoData)
    {
        await DevelopmentDataSeeder.SeedAsync(dbContext);
    }
}

// -------------------------------------------------------
// Middleware
// -------------------------------------------------------

app.UseForwardedHeaders();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// -------------------------------------------------------
// Controllers
// -------------------------------------------------------

app.MapControllers();

app.Run();
