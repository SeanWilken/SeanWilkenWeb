# 🏗️ SeanWilkenWeb Platform Architecture  
*A comprehensive, PDF-style architecture document for the SeanWilkenWeb + XeroEffort platform.*

---

# 📘 Executive Summary

The SeanWilkenWeb platform is a modern, cloud-native, full-stack application deployed on DigitalOcean Kubernetes (DOKS). It combines:

- A Vite + React + Fable client  
- An ASP.NET Core backend  
- A multi-stage Docker build pipeline  
- Kubernetes workloads and services  
- NGINX ingress routing  
- cert-manager + Let`s Encrypt TLS  
- DigitalOcean Managed MongoDB  
- Multi-domain routing and redirects  
- A fully automated deployment workflow  

This document provides a deep architectural overview of the system, explaining how each component works, why it was chosen, and how everything fits together.

---

# 🧱 High-Level Architecture Diagram

```
                          ┌──────────────────────────┐
                          │      DigitalOcean        │
                          │   Kubernetes Cluster     │
                          └─────────────┬────────────┘
                                        │
                        ┌───────────────┴────────────────┐
                        │                                 │
                ┌───────▼────────┐               ┌────────▼────────┐
                │ Ingress-NGINX   │               │ cert-manager     │
                │ (LoadBalancer)  │               │ (Let`s Encrypt)  │
                └───────┬────────┘               └────────┬────────┘
                        │ HTTPS/TLS                           │
                        │                                     │
        ┌───────────────▼────────────────┐                   │
        │ Multi-domain Ingress Rules     │                   │
        │ - seanwilken.com               │                   │
        │ - www.seanwilken.com           │                   │
        │ - xeroeffort.com               │                   │
        │ - www.xeroeffort.com           │                   │
        │ - /shop redirect               │                   │
        └───────────────┬────────────────┘                   │
                        │                                     │
                ┌───────▼────────┐                           │
                │ App Deployment  │                           │
                │ (ASP.NET + SPA) │                           │
                └───────┬────────┘                           │
                        │                                     │
                ┌───────▼────────┐                           │
                │ App Service     │                           │
                └───────┬────────┘                           │
                        │                                     │
                ┌───────▼────────┐                           │
                │ MongoDB (DO)    │◄──────────────────────────┘
                │ Private Network │
                └─────────────────┘
```

---

# 🌐 Core Architectural Components

## 1. DigitalOcean Kubernetes (DOKS)

The platform runs on a managed Kubernetes cluster provided by DigitalOcean.

### Why Kubernetes?
- Declarative deployments  
- Self-healing workloads  
- Horizontal scaling  
- Rolling updates  
- Strong ecosystem (Ingress, cert-manager, etc.)  
- Infrastructure as code (Terraform)  

### Key Features Used
- Node pools  
- LoadBalancer service for ingress  
- Private networking for MongoDB  
- Automatic upgrades  

---

## 2. NGINX Ingress Controller

The ingress controller is the entry point for all external traffic.

### Responsibilities
- Terminate TLS  
- Route traffic to the correct domain  
- Apply redirects  
- Serve static assets  
- Enforce security headers  
- Integrate with cert-manager  

### Why NGINX?
- Most widely used ingress controller  
- Excellent documentation  
- Supports advanced routing  
- Works seamlessly with cert-manager  

---

## 3. cert-manager + Let`s Encrypt

cert-manager automates TLS certificate issuance and renewal.

### Features
- ACME HTTP-01 challenges  
- Multi-domain certificates  
- Automatic renewal  
- Integration with ingress annotations  

### Why Let`s Encrypt?
- Free  
- Automated  
- Trusted by all browsers  

---

## 4. Multi-Domain Ingress Routing

The platform supports four domains:

- seanwilken.com  
- www.seanwilken.com  
- xeroeffort.com  
- www.xeroeffort.com  

### Routing Rules
- `/` → main application  
- `/shop` → redirect to xeroeffort.com/shop  
- xeroeffort.com → redirect to /shop  

### Why this design?
- Clean separation of personal brand vs. shop  
- SEO-friendly canonical URLs  
- Predictable user experience  

---

## 5. Application Deployment (ASP.NET + SPA)

The application is a hybrid:

- ASP.NET Core backend  
- Vite-built React/Fable SPA frontend  

### Why this architecture?
- Strong backend capabilities (C#, F#)  
- Modern frontend tooling (Vite, React, Fable)  
- SPA served directly by ASP.NET static file middleware  
- Simple deployment model  

---

## 6. Multi-Stage Docker Build

The Dockerfile uses three stages:

1. **Client build** (Vite)  
2. **Server build** (dotnet publish)  
3. **Final runtime image**  

### Benefits
- Small final image  
- Reproducible builds  
- Clear separation of concerns  
- No dev dependencies in production  

---

## 7. DigitalOcean Managed MongoDB

MongoDB is provisioned and managed by DigitalOcean.

### Features
- Automated backups  
- Private VPC networking  
- Connection string injected into Kubernetes  
- No maintenance overhead  

### Why Managed Mongo?
- Reliability  
- Security  
- No need to manage replica sets manually  

---

# 🔐 Security Architecture

## TLS Everywhere
- All domains enforce HTTPS  
- HSTS recommended  
- cert-manager handles renewal  

## Private Networking
- MongoDB accessible only inside DO VPC  
- No public DB exposure  

## Ingress Security Headers
Recommended:

```
Strict-Transport-Security: max-age=63072000; includeSubDomains; preload
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
```

## Container Security
- Multi-stage builds  
- Non-root user (recommended future enhancement)  
- Read-only filesystem (optional)  

---

# 🔄 Deployment Architecture

## Build Pipeline
- Vite builds SPA  
- dotnet publish builds backend  
- Docker multi-stage builds final image  
- Image pushed to DOCR  

## Deployment Pipeline
- Kustomize overlays applied  
- Deployment rolled out  
- Ingress updated  
- cert-manager validates TLS  
- Pods restart gracefully  

---

# 🧪 Development Architecture

## Local Dev Stack
- Podman Compose for MongoDB  
- dotnet watch for backend  
- Vite dev server for frontend  
- Automatic environment variable loading  

## Why Podman?
- Docker-compatible  
- Rootless containers  
- Better security model  

---

# 🧭 Future Enhancements

- Horizontal Pod Autoscaling  
- Redis cache  
- Observability stack (Prometheus + Grafana)  
- Canary deployments  
- Feature flags  
- Multi-tenant support  
- CI/CD automation  

---

# 🎉 Summary

This architecture is:

- Modern  
- Secure  
- Scalable  
- Documented  
- Future-proof  
- Developer-friendly  

It balances simplicity with power, making it ideal for both personal projects and production-grade applications.

```
