# Mock Interview Platform

A full-stack application for preparing, conducting, and reviewing technical interview practice sessions. Interviewers manage candidates, schedule and complete interviews, record Feedback, and review reports. Candidates access their own dashboard, interviews, Feedback, and progress.

## Tech Stack

- Angular 19, TypeScript, and SCSS
- ASP.NET Core / .NET 9 and EF Core 9
- PostgreSQL (version 17 in local Compose)
- Okta OIDC with backend-managed cookie authentication
- Docker / Docker Compose and Nginx
- GKE / Cloud SQL, with Artifact Registry and Google Secret Manager configuration

## Run Locally with Docker

1. **Prerequisites:** Git, Docker Desktop with Linux containers and Compose v2, and available ports 4200, 5207, and 5433. You also need access to the configured Okta application and its client secret. Compose already supplies a non-secret authority/client ID; another Okta application requires matching configuration, not just a different secret. Users must have exactly one recognized group membership: `Candidates` or `Interviewers`. See [Okta setup details](docs/DEPLOYMENT.md#local-okta-requirements) for callback and claim requirements.

2. **Clone the repository:**

   ```sh
   git clone https://github.com/RadiCarolAlin/mock-interview-platform.git
   cd mock-interview-platform
   ```

3. **Supply the Okta secret privately.** Set `OKTA_CLIENT_SECRET` in your shell environment, or create a root `.env` file with the following placeholder replaced by your actual value:

   ```dotenv
   OKTA_CLIENT_SECRET=<supply-privately>
   ```

   `.env` is ignored by Git. Never commit the real value. The checked-in Compose database credentials are development defaults, not production secret management.

4. **Start the complete stack from the repository root:**

   ```sh
   docker compose up --build
   ```

   This builds/starts PostgreSQL, the ASP.NET Core API, and the Angular/Nginx frontend. Use the root Compose file; `backend/docker-compose.yml` contains only the API/database arrangement. Nginx uses `API_UPSTREAM=api:8080` locally.

5. **Open the application:**

   | Endpoint | URL |
   |---|---|
   | Frontend / public login | http://localhost:4200/login |
   | API base | http://localhost:5207/api |
   | Anonymous health check | http://localhost:5207/api/health |

   Click **Sign in with Okta** to authenticate. The API base is a route prefix, not a standalone page. PostgreSQL is also exposed on host port 5433.

6. **Stop the stack:**

   ```sh
   docker compose down
   ```

   The named PostgreSQL volume is retained. **`docker compose down -v` deletes the local PostgreSQL volume and its data.**

## Database Initialization and Migrations

On first startup:

1. Compose starts PostgreSQL and waits for its `pg_isready` health check to pass before starting the API.
2. The API calls `Database.MigrateAsync()` in `Program.cs`, automatically applying pending EF Core migrations to create/update the schema. Current migrations are `InitialCreate` and `AddDomainIntegrityChecks`.
3. Demo seeding runs only when **the environment is Development AND `SeedDemoData` is true**. The root Compose environment and checked-in Development settings satisfy both conditions. The seeder reconciles missing demo records while preserving existing data.
4. The API begins serving requests after initialization succeeds. Frontend startup order does not independently guarantee that API initialization has finished.

**You do not need to run `dotnet ef database update` manually for this startup flow.** Database connectivity and migration permissions are required; initialization failures appear in API logs.

PostgreSQL data persists across restarts in the `interview_postgres_data` named volume. The checked-in GKE deployment disables production demo seeding; API startup still applies migrations there.

## Run Tests

Backend, from the repository root with the .NET 9 SDK:

```sh
dotnet test backend/InterviewPractice.sln
```

Frontend, with Node.js/npm and Chrome installed:

```sh
cd frontend
npm ci
npm test -- --watch=false --browsers=ChromeHeadless
```

The suite contains 15 backend test cases and 8 frontend tests. See [Testing Strategy](docs/TESTING.md) for coverage, prerequisites, build commands, and runtime verification boundaries.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [How the Application Works](docs/HOW-IT-WORKS.md)
- [Testing Strategy](docs/TESTING.md)
- [Deployment: local and cloud](docs/DEPLOYMENT.md)
- [System Overview](docs/diagrams/system-overview.md)
- [Database ERD](docs/diagrams/database-erd.md)
- [Deployment Architecture](docs/diagrams/deployment-architecture.md)

## Live Application

Configured public URL: **[https://mock-interview-carol.duckdns.org](https://mock-interview-carol.duckdns.org)**.

The hostname is verified from repository ingress and API configuration. Current availability, certificate status, and deployed version have not been verified by this documentation task.

## Known Limitations

- No user-facing cancel/start interview flow, despite corresponding domain states.
- External Okta group changes require refreshed principal claims.
- Candidate creation is application-local only: it creates a pending local identity
  and Candidate profile for demonstration purposes. It does not provision a real
  Okta account. A real Okta user can later be linked to the pending local identity
  through the application's protected identity-linking flow.
- No end-to-end, live PostgreSQL/cloud, or concurrency tests.
