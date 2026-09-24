# Deployment Overview

The repository supplies two deployment arrangements:

- **Local:** root [docker-compose.yml](../docker-compose.yml) starts an Angular/Nginx frontend, ASP.NET Core API, and PostgreSQL 17 with a named data volume.
- **Cloud:** [k8s](../k8s/) declares GKE workloads/services, a GCE Ingress and managed certificate, an API Pod with Cloud SQL Auth Proxy, and configuration for Google Secret Manager. Images reference Artifact Registry. Okta remains external in both arrangements.

The configured public origin is [https://mock-interview-carol.duckdns.org](https://mock-interview-carol.duckdns.org), confirmed by ingress, certificate, and API frontend-base-URL settings. Repository configuration does not prove current availability, certificate activation, DNS resolution, or successful deployment.

See the [cloud topology](diagrams/deployment-architecture.md). Provisioning a GKE cluster, Artifact Registry repository, Cloud SQL instance/database/user, static IP, DNS, Secret Manager secrets, and cloud IAM permissions is an external prerequisite. Those resources are referenced, not fully provisioned by the checked-in manifests.

# Local Deployment

Prerequisites: Git, Docker with Compose v2 and Linux-container support, free host ports 4200/5207/5433, and access to a suitable Okta OIDC application. A local .NET/Node installation is not required for Docker builds.

1. Clone the repository and enter its root:

   ```sh
   git clone https://github.com/RadiCarolAlin/mock-interview-platform.git
   cd mock-interview-platform
   ```

2. Supply `OKTA_CLIENT_SECRET` through your shell environment or an untracked root `.env` file. Use the real value privately, never in a committed document. The placeholder below must be replaced locally:

   ```dotenv
   OKTA_CLIENT_SECRET=<supply-privately>
   ```

3. Run from the root:

   ```sh
   docker compose up --build
   ```

4. Open `http://localhost:4200/login` and explicitly select **Sign in with Okta**.

| Service | Host access | Container port |
|---|---|---:|
| Frontend | `http://localhost:4200` | 80 |
| API | `http://localhost:5207` | 8080 |
| PostgreSQL | `localhost:5433` | 5432 |
| Development Swagger | `http://localhost:5207/swagger/index.html` | API 8080 |

PostgreSQL's Compose health check gates API startup. The API migrates the database and, with the checked-in Development settings, runs the demo seeder. Frontend `depends_on` establishes startup order but is not an API-readiness guarantee. Data persists in the `interview_postgres_data` named volume.

Use the **root** Compose file for the full application. `backend/docker-compose.yml` is a separate API/database-only arrangement with overlapping container names/ports; do not start both arrangements together.

Both Compose files contain literal development database credentials. They are not reproduced here and should not be treated as secret-managed production credentials. `.gitignore` excludes `.env` files, but ignoring files does not protect a value already tracked elsewhere.

## Local Okta Requirements

Compose already sets a non-secret authority and client ID. `OKTA_CLIENT_SECRET` must belong to that OIDC client. An evaluator without access to that application needs an owner-approved environment/configuration change supplying their own matching authority, client ID, and secret; setting the secret alone does not select a different tenant.

The API implements Authorization Code flow and expects identity/email claims plus exactly one recognized group membership, `Candidates` or `Interviewers`. The Okta application must allow the user and supply `groups` claims to the resulting principal. Exact tenant-side assignments/claim mappings are not exported in this repository.

When entering through Compose's frontend proxy at port 4200, the expected callback addresses are `http://localhost:4200/signin-oidc` and `http://localhost:4200/signout-callback-oidc`. If entering authentication directly through the API at port 5207, callbacks use that origin instead. Register the addresses for the path actually used in the external Okta app; repository inspection cannot confirm existing registrations. The frontend return destination defaults to `http://localhost:4200`.

# Frontend Runtime Proxy Configuration

[frontend/Dockerfile](../frontend/Dockerfile) builds Angular with Node 22, then serves the browser build using Nginx. It copies [nginx.conf.template](../frontend/nginx.conf.template) to `/etc/nginx/templates/default.conf.template`. The standard Nginx image startup substitutes `API_UPSTREAM` into the generated configuration.

| Environment | Runtime value |
|---|---|
| Docker Compose | `API_UPSTREAM=api:8080` |
| GKE frontend Deployment | `API_UPSTREAM=mock-interview-api:80` |

Angular uses relative API URLs. The frontend image therefore does not contain a hardcoded Kubernetes API service name and can be reused in both environments without separate local/cloud builds. Set the variable before starting the container; the Dockerfile does not define a default upstream.

Nginx proxies `/api/`, `/signin-oidc`, and `/signout-callback-oidc`, and uses SPA fallback for frontend routes. In the checked-in GKE ingress, API/callback paths go **directly to the API Service**, bypassing frontend Nginx; its proxy configuration remains available for traffic that does enter through the frontend container.

# Container Images

From the repository root, the correct build contexts are:

```sh
docker build -t mock-interview-frontend:latest ./frontend
docker build -t mock-interview-api:latest -f backend/InterviewPractice.Api/Dockerfile ./backend
```

The backend Dockerfile restores/publishes the API and its production project references using the .NET 9 SDK, then runs it on the ASP.NET Core 9 image at port 8080. Its default environment is Development; the GKE Deployment explicitly overrides it to Production.

The manifests reference this Artifact Registry repository:

```text
europe-central2-docker.pkg.dev/mock-interview-platform-carol/mock-interview-repo
```

Image names are `mock-interview-api:latest` and `mock-interview-frontend:latest`. Credentials for registry access come from the operator's/cloud runtime's configured identity, not values in this guide.

# Kubernetes

| File in `k8s/` | Responsibility |
|---|---|
| [namespace.yaml](../k8s/namespace.yaml) | Creates namespace `mock-interview`. |
| [frontend-deployment.yaml](../k8s/frontend-deployment.yaml) | One frontend replica, runtime upstream, resource requests/limits, image pull policy Always. |
| [frontend-service.yaml](../k8s/frontend-service.yaml) | ClusterIP `mock-interview-frontend`, port 80 → Pod port 80. |
| [api-deployment.yaml](../k8s/api-deployment.yaml) | One API Pod replica with API and database proxy containers; Production configuration. |
| [api-service.yaml](../k8s/api-service.yaml) | ClusterIP `mock-interview-api`, port 80 → API port 8080; associates BackendConfig. |
| [api-service-account.yaml](../k8s/api-service-account.yaml) | Kubernetes ServiceAccount `mock-interview-api`, used by the API Pod. |
| [api-backend-config.yaml](../k8s/api-backend-config.yaml) | GKE load-balancer HTTP health check at `/api/health`, port 8080. |
| [ingress.yaml](../k8s/ingress.yaml) | GCE ingress, named static IP and hostname; routes API/callbacks to API Service and `/` to frontend Service. |
| [managed-certificate.yaml](../k8s/managed-certificate.yaml) | Requests a Google-managed certificate for the configured hostname. |

There are no Kubernetes application liveness/readiness probes in these Deployments. The BackendConfig is a load-balancer health check, not a Pod readiness probe. No cluster creation or IAM-binding manifests are supplied. The ServiceAccount file alone does not establish permission to access Google services.

# API Pod

```text
API Pod (ServiceAccount: mock-interview-api)
├── api: ASP.NET Core, HTTP :8080
└── cloud-sql-proxy: Cloud SQL Auth Proxy 2.18.2, :5432
```

Containers in the Pod share networking. The API connects to `127.0.0.1:5432`; the proxy targets `mock-interview-platform-carol:europe-central2:mock-interview-db`. The configured database/user names are `interview_practice` and `interview_user`. The database password is still required: using Cloud SQL Auth Proxy does not remove database-user authentication.

# Secrets

With `Secrets__Provider=GoogleSecretManager`, `Program.cs` uses project `mock-interview-platform-carol` and reads latest versions of secret IDs `db-password` and `okta-client-secret`. It assembles the database connection string from those credentials and `Database__*` settings. Non-secret authority/client ID and `Frontend__BaseUrl` are supplied as Deployment environment variables.

Google client libraries and the Cloud SQL proxy need an appropriately authorized runtime identity. The repository does not include a service-account private key or fully specify cloud IAM/Workload Identity bindings. Verify these externally rather than assuming the Kubernetes ServiceAccount is sufficient. Do not commit credential files or print secret contents while troubleshooting.

# Database Migrations

`Program.cs` awaits `Database.MigrateAsync()` during API startup in both environments. Checked-in migrations are `20260923004050_InitialCreate` and `20260924042230_AddDomainIntegrityChecks`. The database must be reachable and the configured user must have the required schema permissions. A failure prevents normal API startup.

Demo seeding requires both Development and `SeedDemoData=true`. GKE sets Production and `SeedDemoData=false`, so normal cloud startup applies migrations but does not seed demo records. Application startup is the current migration mechanism, not a separate migration Job.

# Health Checks

`GET /api/health` is anonymous and returns HTTP 200 with `{ "status": "Healthy" }`. It does not actively check PostgreSQL or Okta. The application has no root `/health` endpoint.

```sh
curl -f http://localhost:5207/api/health
curl -f https://mock-interview-carol.duckdns.org/api/health
```

# Deployment / Update Procedure

This is an operator-run example, not an infrastructure-provisioning script. Authenticate `gcloud`, select the intended project, and obtain Kubernetes credentials for the existing cluster first. The cluster name/location is not provided by the repository.

1. Build the two images with the commands above.
2. Configure registry authentication, tag, and push:

   ```sh
   gcloud auth configure-docker europe-central2-docker.pkg.dev
   docker tag mock-interview-api:latest europe-central2-docker.pkg.dev/mock-interview-platform-carol/mock-interview-repo/mock-interview-api:latest
   docker tag mock-interview-frontend:latest europe-central2-docker.pkg.dev/mock-interview-platform-carol/mock-interview-repo/mock-interview-frontend:latest
   docker push europe-central2-docker.pkg.dev/mock-interview-platform-carol/mock-interview-repo/mock-interview-api:latest
   docker push europe-central2-docker.pkg.dev/mock-interview-platform-carol/mock-interview-repo/mock-interview-frontend:latest
   ```

3. Apply namespace first, then resources from the repository root:

   ```sh
   kubectl apply -f k8s/namespace.yaml
   kubectl apply -f k8s/
   ```

4. When updating unchanged `:latest` references, restart workloads so new Pods pull the new images. Applying identical YAML alone does not trigger rollout:

   ```sh
   kubectl -n mock-interview rollout restart deployment/mock-interview-api deployment/mock-interview-frontend
   kubectl -n mock-interview rollout status deployment/mock-interview-api
   kubectl -n mock-interview rollout status deployment/mock-interview-frontend
   ```

5. Inspect and smoke-check:

   ```sh
   kubectl -n mock-interview get pods,services,ingress
   kubectl -n mock-interview get managedcertificate mock-interview-certificate
   curl -f https://mock-interview-carol.duckdns.org/api/health
   ```

6. Open `/login`; manually verify explicit Okta login, both role dashboards, a permitted API operation, Candidate ownership, and logout returning to login. These checks are not automated by the deployment manifests.

# Troubleshooting

- **CrashLoopBackOff:** use `kubectl -n mock-interview describe pod <pod-name>` and `kubectl -n mock-interview logs <pod-name> -c api --previous`. Startup configuration, secret access, proxy connectivity, and migrations can prevent startup.
- **Cloud SQL connectivity:** inspect `kubectl -n mock-interview logs <pod-name> -c cloud-sql-proxy`; verify runtime identity permissions, instance connection name, and database-user credentials without printing passwords.
- **Nginx upstream resolution:** check `kubectl -n mock-interview logs deployment/mock-interview-frontend -c frontend`, the API Service name/port, and `API_UPSTREAM`. Locally inspect `docker compose logs frontend api` and use `api:8080`, not a Kubernetes-only DNS name.
- **Local API not ready:** inspect `docker compose ps` and `docker compose logs api postgres`; frontend startup order does not guarantee API readiness.
- **Healthy Pod but failing public requests:** check ingress/certificate status, DNS/static IP, service routing, and `/api/health`. Rollout success alone does not prove external readiness.
- **OIDC callback errors:** verify the actual origin and callback registrations. Nginx forwards its own `$scheme`, and the API accepts forwarded headers with cleared trusted-network/proxy lists; the real proxy/trust boundary requires runtime verification.
