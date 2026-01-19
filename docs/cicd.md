# 🔄 CI/CD Pipeline  
*A complete overview of the build, test, containerization, and deployment pipeline for the SeanWilkenWeb platform.*

This document explains how code moves from your local machine → to a Docker image → to DigitalOcean Container Registry → to Kubernetes → to production.

It includes:

- Pipeline stages  
- ASCII diagrams  
- Deployment flow  
- Branching strategy  
- Release strategy  
- Future enhancements  

---

# 🧱 Overview

The CI/CD pipeline for this platform is intentionally simple, modular, and cloud‑agnostic. It can run:

- Locally (via scripts)  
- In GitHub Actions  
- In Azure DevOps  
- In any CI system that supports Docker + kubectl  

The pipeline has three major phases:

1. **Build**  
2. **Containerize**  
3. **Deploy**  

---

# 🧩 Pipeline Stages

## 1. Build Stage

### Client Build (Vite)
- Installs dependencies  
- Runs TypeScript/Fable compilation  
- Produces hashed assets in `/deploy/public`  

### Server Build (ASP.NET Core)
- Restores NuGet packages  
- Compiles server  
- Runs tests (optional)  
- Produces publish output  

### Why separate builds?
- Faster incremental builds  
- Clearer caching  
- Smaller final image  

---

## 2. Docker Build Stage

The multi‑stage Dockerfile:

1. **Client build stage**  
2. **Server build stage**  
3. **Final runtime stage**

### Benefits
- No dev dependencies in production  
- Small final image  
- Reproducible builds  
- Clean separation of concerns  

### Output
A single production‑ready image tagged with:

- Commit SHA  
- Branch name  
- `latest` (optional)

---

## 3. Push to DOCR

The built image is pushed to:

```
registry.digitalocean.com/<registry>/<image>
```

Kubernetes pulls from this registry during deployment.

---

## 4. Deployment Stage

Deployment uses:

- Kustomize overlays  
- Kubernetes manifests  
- Rolling updates  
- Zero downtime  

### Steps
1. Apply manifests  
2. Update deployment image tag  
3. Kubernetes rolls out new pods  
4. Old pods drain gracefully  
5. Ingress continues routing traffic  

---

# 🗺️ ASCII Diagram: Full CI/CD Pipeline

```
┌──────────────────────────────┐
│          Developer           │
│        (Local Machine)       │
└───────────────┬──────────────┘
                │ git push
                ▼
        ┌───────────────────┐
        │      CI/CD        │
        │   (GitHub/Azure)  │
        └─────────┬─────────┘
                  │
     ┌────────────┴────────────┐
     │                           │
┌────▼────┐               ┌─────▼─────┐
│  Build  │               │   Test     │
│ (Vite)  │               │ (optional) │
└────┬────┘               └─────┬─────┘
     │                          │
┌────▼──────────────────────────▼─────┐
│         Docker Build (multi-stage)  │
└────┬──────────────────────────┬─────┘
     │                          │
     ▼                          ▼
┌──────────────┐        ┌──────────────────┐
│   Tag Image   │        │   Push to DOCR   │
└──────┬────────┘        └────────┬────────┘
       │                            │
       ▼                            ▼
┌──────────────────────────────────────────┐
│           Kubernetes Deployment          │
│      (Kustomize + Rolling Update)        │
└──────────────────────────────────────────┘
```

---

# 🚀 Deployment Flow (Step‑by‑Step)

```
1. Developer commits code
2. CI builds client + server
3. CI builds Docker image
4. CI pushes image to DOCR
5. CI updates Kubernetes manifests
6. CI applies Kustomize overlay
7. Kubernetes rolls out new pods
8. Ingress routes traffic to new pods
9. Old pods terminate gracefully
```

---

# 🌿 Branching Strategy

Recommended:

### `main`
- Always deployable  
- Production releases  

### `dev`
- Staging environment  
- Integration testing  

### Feature branches
- Short‑lived  
- Merged via PR  

---

# 🏷️ Release Strategy

### Production releases
- Triggered by merging into `main`  
- Automatically build + deploy  

### Staging releases
- Triggered by merging into `dev`  

### Hotfixes
- Branch from `main`  
- Merge back into both `main` and `dev`  

---

# 🧪 Local CI/CD Simulation

You can simulate the entire pipeline locally:

```
./scripts/build.ps1
./scripts/deploy.ps1
```

This:

- Builds the Docker image  
- Pushes to DOCR  
- Applies Kubernetes manifests  

---

# 🔧 Kustomize Overlay Structure

```
infrastructure/k8s/
│
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── kustomization.yaml
│
└── overlays/
    └── production/
        ├── ingress-app.yaml
        ├── ingress-shop-redirect.yaml
        ├── ingress-xe-root-redirect.yaml
        └── kustomization.yaml
```

---

# 🛡️ Security in CI/CD

- DOCR requires authentication  
- Kubernetes access restricted via kubeconfig  
- Secrets stored in Kubernetes Secrets  
- No secrets stored in CI logs  
- TLS handled by cert‑manager  

---

# 🔮 Future Enhancements

### 1. Automated Canary Deployments
- Gradual rollout  
- Automatic rollback on failure  

### 2. Image Scanning
- Trivy  
- GitHub Advanced Security  

### 3. Terraform Automation
- `terraform plan` on PR  
- `terraform apply` on merge  

### 4. Observability Integration
- Prometheus alerts  
- Grafana dashboards  

---

# 🎉 Summary

This CI/CD pipeline is:

- Simple  
- Reliable  
- Cloud‑agnostic  
- Easy to extend  
- Production‑ready  

It gives you a clean, predictable path from code → container → cluster → production.

