# Testing Strategy

The suite targets important application behavior rather than a coverage percentage. It contains **15 backend test cases** and **8 frontend tests**. These are isolated tests: they do not require a running API, database server, Okta tenant, or cloud deployment.

# Backend Tests

[InterviewPractice.Tests](../backend/InterviewPractice.Tests/InterviewPractice.Tests.csproj) uses xUnit, the .NET test SDK, and EF Core InMemory. [TestDatabase.cs](../backend/InterviewPractice.Tests/TestDatabase.cs) creates an isolated database name for each test using the real `ApplicationDbContext` and its mappings.

| File | Executed cases | Behavior |
|---|---:|---|
| [RequestValidationTests.cs](../backend/InterviewPractice.Tests/RequestValidationTests.cs) | 6 | Empty/whitespace candidate first names; duration 14/241 rejected and 15/240 accepted; score 0/11 rejected and 1/10 accepted; undefined experience level rejected. Theory rows count as separate cases. |
| [BusinessRulesTests.cs](../backend/InterviewPractice.Tests/BusinessRulesTests.cs) | 6 | Cancelled completion raises `BusinessConflictException` without changing saved status; valid update/completion persists; duplicate feedback and normalized duplicate email are conflicts; missing interview/candidate raises `ResourceNotFoundException` in creation workflows. |
| [IdentityTests.cs](../backend/InterviewPractice.Tests/IdentityTests.cs) | 3 | Missing/ambiguous Okta groups rejected; matching email cannot relink a real identity; Candidate → Interviewer → Candidate role changes retain profiles and interview history. |

Request tests invoke DataAnnotations validation directly. Service tests exercise actual Application services. They do not run ASP.NET middleware or verify the centralized exception handler's HTTP responses.

# Frontend Tests

The existing Jasmine/Karma setup runs in ChromeHeadless. Angular TestBed and mocked HTTP responses exercise components, guards, and helpers; there is no end-to-end framework.

| File | Tests | Behavior |
|---|---:|---|
| [app.component.spec.ts](../frontend/src/app/app.component.spec.ts) | 3 | Public login remains accessible after 401 and starts Okta only on a click; each of the three guards navigates to Angular `/login` on 401 without starting Okta; 403 displays permission failure without login/redirect. |
| [form-validation.spec.ts](../frontend/src/app/form-validation.spec.ts) | 3 | Duration and score limits, fractional values and nonblank text; actual experience-level select produces a number; optional TargetRole is an empty string; a second pending candidate submission does not issue another create call. |
| [api-error.spec.ts](../frontend/src/app/core/utils/api-error.spec.ts) | 2 | Safe 409 ProblemDetails detail; generic 500 message hides server details and network failures get a connectivity message. |

# Running Tests

Install the .NET 9 SDK for backend commands. Frontend commands require a compatible Node.js/npm installation and Google Chrome available to the Karma Chrome launcher. The frontend Docker build uses Node 22; `CHROME_BIN` can point to a nonstandard local Chrome executable when necessary.

From the repository root:

```sh
dotnet test backend/InterviewPractice.sln
dotnet build backend/InterviewPractice.sln
```

From `frontend/`:

```sh
npm ci
npm test -- --watch=false --browsers=ChromeHeadless
npm run build -- --configuration production
```

On Windows PowerShell, `npm.cmd` can be used instead of `npm` if script execution policy blocks `npm.ps1`.

The preceding implementation validation recorded 15/15 backend and 8/8 frontend tests passing, a backend build with zero warnings/errors, and a successful Angular production build. Local npm environment/cache warnings were non-blocking. Documentation changes do not constitute a fresh test run.

# Testing Boundaries

EF Core InMemory verifies application behavior, **not PostgreSQL relational semantics**. It does not establish that foreign keys, unique indexes, CHECK constraints, transactions, SQL translation, or migrations behave correctly against PostgreSQL. Duplicate checks tested here are service checks, not concurrent database-conflict tests.

The suite does not verify live Okta, browser OIDC redirects/cookies, GKE, Cloud SQL, real concurrency, deployment behavior, performance/load, or complete HTTP pipeline behavior. It makes no coverage-percentage claim.

# Manual Verification

No recorded end-to-end manual execution results are supplied by the repository. Treat the following as runtime verification still required, rather than claiming they passed:

- Real Okta sign-in/sign-out and callbacks, expired cookies, and role-specific dashboard redirects.
- Role/group changes after claims refresh, denied access, and Candidate ownership checks through the running API.
- Candidate/interview/feedback flows and database conflicts under simultaneous requests against PostgreSQL.
- Migration and seed startup behavior against the intended database; production seeding remains disabled.
- Ingress, HTTPS certificate, Cloud SQL connectivity, Secret Manager access, and health routing.
- Responsive login/forms, recoverable API/network failures, and browser navigation.

See [Deployment](DEPLOYMENT.md) for smoke-check commands and [How It Works](HOW-IT-WORKS.md) for the expected flows.
