# Deployment Architecture

```mermaid
flowchart TD
    Browser[Internet / User browser] -->|HTTPS| Ingress
    Certificate[Google-managed certificate] -.-> Ingress
    subgraph GKE["GKE: namespace mock-interview"]
        Ingress[GCE Ingress]
        Ingress -->|Frontend paths| FrontService[Frontend Service: ClusterIP port 80]
        Ingress -->|API and OIDC callback paths| ApiService[API Service: ClusterIP port 80]
        FrontService --> FrontContainer
        subgraph FrontPod[Frontend Pod]
            FrontContainer[Nginx: port 80 / Angular static assets]
        end
        ApiService --> ApiContainer
        subgraph ApiPod["API Pod: ServiceAccount mock-interview-api"]
            ApiContainer[ASP.NET Core container: port 8080]
            ApiContainer -->|127.0.0.1:5432| Proxy[Cloud SQL Auth Proxy sidecar]
        end
        FrontContainer -.->|Proxy fallback using API_UPSTREAM| ApiService
    end
    Proxy --> CloudSQL[(Cloud SQL PostgreSQL)]
    ApiContainer -->|Read configured secrets at startup| Secrets[Google Secret Manager]
    Browser <-.->|Login / logout redirects| Okta[Okta]
    ApiContainer <-.->|OIDC protocol| Okta
    Registry[Artifact Registry] -.->|Frontend image pull| FrontContainer
    Registry -.->|API image pull| ApiContainer
```

The diagram reflects the checked-in [Kubernetes manifests](../../k8s/). Services route to Pods; the API and Cloud SQL proxy are separate containers sharing a Pod network. The ingress routes `/api`, `/signin-oidc`, and `/signout-callback-oidc` directly to the API Service. Frontend Nginx can also proxy these paths, but it is not an extra mandatory hop for normal ingress API traffic.

Both Deployments request one replica. The hostname/certificate and Google service references are configured, not proof that those resources are currently ready. Cluster creation, external IAM bindings, DNS/static IP setup, and actual Okta callback registrations are not fully described by the manifests. See [Deployment](../DEPLOYMENT.md) for prerequisites, ports, image names, and operator commands.
