# Mock Interview Platform

A full-stack web application for preparing, conducting, and reviewing mock technical interviews.

The platform allows interviewers to manage candidates, schedule interview sessions, record structured feedback, and review progress over time. Candidates have a dedicated portal where they can review their own interviews, feedback, and progress.

The project was built as a technical assessment with a focus on clean architecture, maintainability, security, containerization, and deployment readiness.

---

## Features

### Interviewer

- Dashboard with interview statistics and recent activity
- Candidate management
- Candidate search
- Create and update candidate profiles
- Schedule mock interviews
- Edit interview details
- Complete interview sessions
- Record structured interview feedback
- Track candidate interview history
- View candidate progress
- Reporting and interview statistics

### Candidate

- Dedicated candidate dashboard
- View own interview history
- View interview details
- Review interviewer feedback
- Track progress across multiple interview sessions

### Authentication & Authorization

Authentication is handled through Okta using OpenID Connect.

The application uses a Backend-for-Frontend style authentication flow:

1. The user accesses the Angular application.
2. Authentication is initiated through the .NET backend.
3. The backend redirects the user to Okta.
4. Okta performs authentication.
5. The backend receives the authorization code.
6. The backend creates an authenticated HTTP-only cookie session.
7. Angular communicates with the backend using the authenticated session.

Authentication tokens are not stored in the browser application.

Two application roles are supported:

- `Interviewer`
- `Candidate`

Authorization policies on the backend protect role-specific API endpoints.

---

## Technology Stack

### Frontend

- Angular
- TypeScript
- SCSS
- Nginx
- Docker

### Backend

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- OpenID Connect
- Cookie Authentication
- Swagger / OpenAPI

### Database

- PostgreSQL 17
- Entity Framework Core Migrations

### Identity

- Okta
- OpenID Connect
- Group-based role mapping

### Infrastructure

- Docker
- Docker Compose
- Nginx
- Google Cloud deployment planned with:
    - Google Kubernetes Engine
    - Artifact Registry
    - Cloud SQL
    - Kubernetes Ingress

---

## Architecture

The repository is organized as a monorepo containing the frontend and backend applications.

```text
mock-interview-platform/
│
├── backend/
│   ├── InterviewPractice.Api/
│   ├── InterviewPractice.Application/
│   ├── InterviewPractice.Domain/
│   ├── InterviewPractice.Infrastructure/
│   └── InterviewPractice.sln
│
├── frontend/
│   ├── src/
│   ├── Dockerfile
│   ├── angular.json
│   └── package.json
│
├── docker-compose.yml
├── .gitignore
└── README.md
```

### Backend Architecture

The backend follows a pragmatic layered architecture.

```text
API
 │
 ▼
Application
 │
 ▼
Domain

Infrastructure
 │
 ├── Entity Framework Core
 ├── PostgreSQL
 └── Persistence
```

Responsibilities are separated between four projects:

### InterviewPractice.Domain

Contains the core domain model:

- Users
- Candidates
- Interviewers
- Interviews
- Feedback
- Domain enums

The Domain project does not depend on infrastructure concerns.

### InterviewPractice.Application

Contains application services, DTOs, and abstractions used to implement application use cases.

Examples include:

- Candidate management
- Interview management
- Feedback management
- Reporting
- Current user resolution

### InterviewPractice.Infrastructure

Contains infrastructure-specific implementations.

Primary responsibilities include:

- Entity Framework Core
- PostgreSQL persistence
- Database configuration
- EF Core migrations
- Development/demo data seeding

### InterviewPractice.Api

Contains the HTTP API and application entry point.

Responsibilities include:

- REST controllers
- Authentication
- Authorization
- Okta OpenID Connect configuration
- CORS
- Swagger
- Dependency injection

---

## Domain Model

The main relationships are:

```text
User
 │
 ├── CandidateProfile
 │       │
 │       └── Interviews
 │               │
 │               └── Feedback
 │
 └── InterviewerProfile
         │
         └── Interviews
```

An interview belongs to one candidate and one interviewer.

A completed interview can have one feedback record containing:

- Overall score
- Strengths
- Areas for improvement
- Outcome
- Additional comments

Supported outcomes include:

- Needs More Practice
- Making Progress
- Ready
- Strong Performance

---

## Running Locally with Docker

### Requirements

Install:

- Docker Desktop
- Git

The application can be started using Docker Compose.

### Okta Secret

The Okta client secret must not be committed to Git.

Create a `.env` file in the repository root:

```env
OKTA_CLIENT_SECRET=your_okta_client_secret
```

The `.env` file is excluded through `.gitignore`.

The remaining local Okta configuration is provided to the backend through configuration/environment variables.

### Start the Application

From the repository root:

```bash
docker compose up --build
```

Docker Compose starts:

```text
Frontend
Angular + Nginx
http://localhost:4200

Backend
.NET 9 API
http://localhost:5207

PostgreSQL
localhost:5433
```

The PostgreSQL container runs internally on port `5432`.

---

## Database Initialization

In the Development environment, the backend automatically applies pending Entity Framework Core migrations during startup.

```text
PostgreSQL starts
        │
        ▼
Backend starts
        │
        ▼
Database.MigrateAsync()
        │
        ▼
DevelopmentDataSeeder
        │
        ▼
API ready
```

This allows a fresh development environment to initialize automatically when running:

```bash
docker compose up --build
```

Development seed data is idempotent and provides several candidate and interview scenarios for demonstrating the application.

Demo data includes:

- Candidates with different experience levels
- Completed interviews
- Scheduled interviews
- Interview feedback
- Different interview outcomes
- Candidate progress history

Demo data is intended only for the Development environment.

---

## Local URLs

| Service | URL |
|---|---|
| Frontend | `http://localhost:4200` |
| Backend API | `http://localhost:5207` |
| Swagger | `http://localhost:5207/swagger/index.html` |
| Health Check | `http://localhost:5207/api/health` |
| PostgreSQL Host Port | `5433` |

---

## Security

The application uses several security measures:

- OpenID Connect authentication through Okta
- Authorization Code flow
- HTTP-only authentication cookies
- Backend authorization policies
- Candidate and Interviewer role separation
- Candidate ownership validation
- API requests return `401` or `403` instead of authentication redirects
- Database credentials and Okta secrets can be supplied through environment configuration
- Sensitive `.env` files are excluded from source control

Candidate endpoints only expose information belonging to the authenticated candidate.

Interviewer-only endpoints are protected using the `Interviewer` authorization policy.

---

## Demo Data and Identity

Candidate business records and authentication identities are intentionally treated as separate concepts.

Not every candidate stored in the application requires an Okta account.

An Okta identity is required only when a candidate needs to authenticate and access the Candidate Portal.

This allows interviewers to manage candidate records without provisioning an identity for every participant.

For demonstration purposes, the environment can contain:

- One authenticated interviewer
- One authenticated candidate
- Additional candidate records used for reporting and interview-management scenarios

---

## API Areas

The backend exposes APIs for:

```text
/api/Candidates
/api/Interviews
/api/Feedback
/api/Reports
/api/candidate/me
/api/health
```

Candidate self-service endpoints resolve the authenticated candidate from the current authenticated user and enforce ownership of requested resources.

---

## Docker Architecture

Local development uses three containers:

```text
                    Docker Compose
                          │
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
     Frontend            API          PostgreSQL
 Angular + Nginx        .NET 9            17
     :4200              :5207            :5433
                          │
                          ▼
                         Okta
                      (external)
```

The frontend and backend use separate Docker images so they can be deployed and scaled independently.

---

## Deployment Architecture

The application is designed to support deployment to Google Cloud.

The intended architecture is:

```text
                       Internet
                           │
                           ▼
                    Kubernetes Ingress
                           │
               ┌───────────┴───────────┐
               │                       │
               ▼                       ▼
         Frontend Service        Backend Service
               │                       │
               ▼                       ▼
         Angular / Nginx           .NET API
            GKE Pod                GKE Pod
                                       │
                                       ▼
                                  Cloud SQL
                                  PostgreSQL

                         Okta
                    External Identity
```

Container images are intended to be stored in Google Artifact Registry and deployed to Google Kubernetes Engine.

Production secrets should be supplied using deployment-level secret management rather than committed configuration files.

---

## Assumptions

The application makes several deliberate assumptions:

- A candidate may participate in multiple mock interview sessions.
- Each interview belongs to exactly one candidate.
- Each interview is conducted by one interviewer.
- Feedback is recorded after an interview has been completed.
- Each interview can contain one feedback record.
- Candidates can only access their own interview information.
- Interviewers manage candidates, interviews, feedback, and reports.
- Authentication identities and candidate business records are separate concerns.

---

## Current Limitations

The current implementation focuses on the primary assessment requirements.

Known limitations include:

- Candidate identity provisioning in Okta is not automated.
- Okta users and groups are currently managed externally through Okta administration.
- Feedback editing is not currently implemented.
- Reporting focuses on core interview and candidate metrics.
- Development demo data is intended for demonstration purposes only.
- Production observability and monitoring are not yet configured.

---

## Future Improvements

Potential improvements include:

- Automatic candidate provisioning through the Okta Management API
- Automated assignment to Okta groups
- Candidate invitation and activation workflow
- Feedback editing
- Advanced reporting and trend visualization
- Interviewer management
- Pagination for larger datasets
- Advanced filtering and search
- Audit logging
- Automated integration tests
- End-to-end tests
- CI/CD with GitHub Actions
- Kubernetes autoscaling
- Centralized logging and monitoring
- Production secret management

---

## Design Decisions

### Backend-for-Frontend Authentication

The Angular application does not manage OAuth tokens directly.

Authentication is handled by the backend and represented in the browser through an HTTP-only cookie.

This reduces exposure of access tokens to browser-side JavaScript and centralizes authentication and authorization behavior in the backend.

### Separate Frontend and Backend Containers

Although the project is maintained in a single repository, the frontend and backend are built as separate container images.

This enables:

- Independent deployments
- Independent scaling
- Smaller deployment units
- Clear service boundaries

### PostgreSQL

PostgreSQL is used as the relational persistence layer and is accessed through Entity Framework Core.

### Pragmatic Architecture

The backend intentionally avoids unnecessary architectural complexity.

The solution uses clear Domain, Application, Infrastructure, and API boundaries without introducing additional patterns unless required by the application.

---

## Repository

This repository contains the complete frontend, backend, Docker configuration, and deployment-related resources for the Mock Interview Platform.