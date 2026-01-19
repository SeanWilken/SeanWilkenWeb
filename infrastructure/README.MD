Absolutely, Sean — this is a great moment to consolidate everything you’ve built. You now have a fully functioning, multi‑domain, TLS‑secured, Kubernetes‑deployed SAFE/React application with clean ingress routing, redirects, cert‑manager automation, and a DigitalOcean‑backed infrastructure. A solid README will make this whole system understandable to future you and anyone else who touches the repo.

Below is a polished, production‑grade **Infrastructure README** that reflects everything we’ve configured so far. It’s written to be clear, auditable, and onboarding‑friendly — the kind of document you’d expect in a professional platform engineering repo.

---

# 🏗️ **Infrastructure Overview**

This directory contains the full infrastructure-as-code setup for deploying the SeanWilkenWeb application to DigitalOcean Kubernetes (DOKS). It includes:

- Terraform configuration for provisioning cloud resources  
- Kubernetes manifests for application deployment  
- Ingress configuration for multi‑domain routing and redirects  
- TLS automation via cert‑manager + Let’s Encrypt  
- A Docker-based build pipeline for the application  
- Environment variable and secret management  
- Production-ready networking, security, and routing behavior  

The goal is a reproducible, secure, and maintainable deployment pipeline that can be applied end‑to‑end with a single Terraform apply and a kubectl deploy.

---

# 📁 **Repository Structure**

```
infrastructure/
│
├── terraform/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── kubernetes.tf
│   ├── digitalocean.tf
│   ├── registry.tf
│   ├── mongodb.tf
│   └── ...
│
├── k8s/
│   ├── base/
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   ├── namespace.yaml
│   │   └── kustomization.yaml
│   │
│   └── overlays/
│       └── production/
│           ├── ingress-app.yaml
│           ├── ingress-shop-redirect.yaml
│           ├── ingress-xe-root-redirect.yaml
│           ├── kustomization.yaml
│           └── ...
│
└── scripts/
    ├── deploy.ps1
    ├── build.ps1
    └── kubeconfig-setup.ps1
```

---

# 🌐 **DigitalOcean Infrastructure (Terraform)**

Terraform provisions all cloud resources required for production:

### **1. Kubernetes Cluster (DOKS)**
- Region: configurable (default: NYC3)
- Node pool: size, count, and version configurable
- Auto-upgrades enabled
- Auto-scaling optional

### **2. Container Registry**
- DigitalOcean Container Registry  
- Used for pushing application images  
- Integrated with DOKS for seamless image pulls  

### **3. Managed MongoDB Cluster**
- DigitalOcean Managed MongoDB  
- Database + user + connection string output  
- Private networking enabled  
- Connection string injected into Kubernetes secrets  

### **4. DNS Records**
Terraform can optionally manage DNS:
- A and CNAME records for:
  - `seanwilken.com`
  - `www.seanwilken.com`
  - `xeroeffort.com`
  - `www.xeroeffort.com`

---

# 🚢 **Application Deployment (Kubernetes)**

The application is deployed using Kustomize overlays.

### **Base Manifests**
- Deployment  
- Service  
- Namespace  
- ConfigMap / Secrets (if needed)

### **Production Overlay**
Adds:
- Ingress rules  
- TLS configuration  
- Domain routing  
- Redirect logic  
- Environment-specific settings  

Deployment flow:

```
kubectl apply -k infrastructure/k8s/overlays/production
```

---

# 🔐 **TLS & Certificate Management**

TLS is fully automated using:

- **cert-manager**
- **Let’s Encrypt (Production issuer)**
- **Single multi-domain certificate**

Domains covered:

- seanwilken.com  
- www.seanwilken.com  
- xeroeffort.com  
- www.xeroeffort.com  

Certificate is stored as:

```
wilkenweb-multi-tls
```

Auto-renews every 60 days.

---

# 🌍 **Ingress Routing & Redirects**

The ingress layer is powered by **NGINX Ingress Controller**.

### **1. App Ingress (Primary)**
Handles all traffic for:

- seanwilken.com  
- www.seanwilken.com  
- xeroeffort.com  
- www.xeroeffort.com  

Routes:
- `/` → app service  
- `/shop` → app service (placeholder so redirect ingress can override)

### **2. Shop Redirect Ingress**
Redirects:

```
/shop → https://xeroeffort.com/shop
```

For all domains.

Uses:

```
nginx.ingress.kubernetes.io/permanent-redirect
```

### **3. XeroEffort Root Redirect**
Redirects:

```
xeroeffort.com → xeroeffort.com/shop
www.xeroeffort.com → xeroeffort.com/shop
```

This ensures the shop is the canonical landing page for the xeroeffort brand.

---

# 🐳 **Docker Build Pipeline**

The application is built using a multi-stage Dockerfile:

### **Stage 1 — Client Build**
- Vite build  
- Tailwind + React  
- Outputs hashed assets to `/deploy/public`

### **Stage 2 — Server Build**
- ASP.NET Core server  
- Serves static files from `/app/public`  
- Listens on internal HTTP (`http://+:5000`)  
- TLS terminated at ingress  

### **Stage 3 — Final Runtime Image**
- Lightweight runtime  
- Contains:
  - Built server  
  - Built client assets  
- Exposes port 5000 internally  

Images are pushed to DOCR and pulled by Kubernetes.

---

# 🔧 **Environment Variables & Secrets**

### **Kubernetes Secrets**
- MongoDB connection string  
- API keys (if needed)  
- Environment-specific settings  

### **ASP.NET Core**
```
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Production
```

### **Client Environment**
Vite loads env from:

```
infrastructure/.env.production
```

---

# 🛡️ **Security Hardening**

### **Ingress Security Headers**
(Optional but recommended)

- HSTS  
- X-Frame-Options  
- X-Content-Type-Options  
- X-XSS-Protection  

### **TLS Everywhere**
All domains enforce HTTPS.

### **No Mixed Content**
All assets served securely.

### **Private Networking**
MongoDB accessible only inside DO VPC.

---

# 🚀 **Deployment Workflow**

### **1. Build & Push Image**
```
./scripts/build.ps1
```

### **2. Apply Terraform (if infra changed)**
```
cd infrastructure/terraform
terraform init
terraform apply
```

### **3. Deploy to Kubernetes**
```
kubectl apply -k infrastructure/k8s/overlays/production
```

### **4. Verify**
```
kubectl get pods -n wilkenweb-prod
kubectl get ingress -n wilkenweb-prod
```

---

# 🧭 **Future Enhancements**

- Horizontal Pod Autoscaling  
- Redis cache  
- Canary deployments  
- CI/CD pipeline  
- Observability stack (Prometheus + Grafana)  
- WAF / rate limiting  

---

If you want, I can also generate:

- A **developer onboarding guide**  
- A **deployment troubleshooting guide**  
- A **diagram** of the full infrastructure  
- A **MongoDB connection troubleshooting section**  
- A **CI/CD pipeline plan**  

Just say the word and we’ll keep leveling this up.


🚀 Production Deployment Guide (DigitalOcean + Kubernetes + Terraform)
This project uses a Git‑SHA‑driven deployment pipeline to build, push, and deploy the SAFE stack application to DigitalOcean Kubernetes.
Overview
The deployment pipeline:
- Builds the Docker image using the current Git commit SHA
- Pushes the image to DigitalOcean Container Registry (DOCR)
- Applies Terraform to update secrets, config, and the Kustomize overlay
- Applies the Kubernetes manifests via Kustomize
- Waits for the rollout
- Performs a health check
This ensures every deployment is:
- Traceable
- Reproducible
- Rollback‑friendly
- Environment‑safe

1. Prerequisites
Install:
- Podman or Docker
- Terraform
- kubectl
- Git
- DigitalOcean CLI (optional)
Authenticate:
doctl auth init


Ensure you have:
- A DOCR registry
- A Kubernetes cluster
- A Managed MongoDB instance

2. Environment Variables
Create:
infrastructure/docker/App/.env


With:
VITE_STRIPE_API_PK=pk_live_123
VITE_API_BASE_URL=https://seanwilken.com/api
SERVER_PROXY_PORT=5000


These are build‑time variables used by Vite.
Runtime secrets (Stripe SK, Mongo URI, email credentials) are managed by Terraform → Kubernetes Secrets.

3. Deploying
Run:
./production.ps1


This will:
- Compute the Git SHA
- Build the Docker image
- Push to DOCR
- Apply Terraform with the SHA
- Apply Kustomize
- Wait for rollout
- Run a health check

4. Rollbacks
To roll back to a previous version:
terraform apply -var="tag=<old_sha>"
kubectl apply -k infrastructure/k8s/overlays/prod



5. Kustomize Image Tagging
The overlay contains:
patchesStrategicMerge:
  - patch-image.yaml


Where Terraform injects:
image: registry.digitalocean.com/wilkenweb/app:<sha>



6. Secrets
Runtime secrets are managed via Terraform:
- Stripe Secret Key
- Mongo Connection String
- Email credentials
- Printful OAuth key
These are injected into the Deployment via:
envFrom:
  - secretRef:
      name: app-secrets



# 🧭 **Developer Onboarding Guide**

## 🎒 Purpose  
This guide helps new developers (or future you) get productive quickly with the SeanWilkenWeb infrastructure. It covers environment setup, local development, deployment workflow, and debugging patterns.

---

## 🖥️ 1. **Prerequisites**

### Required tools  
- Docker Desktop  
- .NET SDK  
- Node.js + PNPM  
- kubectl  
- Terraform  
- DigitalOcean CLI (optional but helpful)  
- PowerShell (for scripts)  

### Accounts  
- DigitalOcean account  
- Access to DO Container Registry  
- Access to DO Kubernetes cluster  

---

## 🧪 2. **Local Development**

### Start the client  
```
cd src/Client
pnpm install
pnpm dev
```

### Start the server  
```
cd src/Server
dotnet run
```

### Local API proxy  
Vite proxies `/api/*` to the server automatically.

---

## 🐳 3. **Local Docker Build**

```
./scripts/build.ps1
```

This produces a production-ready multi-stage image identical to what runs in Kubernetes.

---

## ☸️ 4. **Kubernetes Access**

### Set up kubeconfig  
```
./scripts/kubeconfig-setup.ps1
```

### Verify access  
```
kubectl get pods -n wilkenweb-prod
```

---

## 🚀 5. **Deploy to Production**

```
./scripts/deploy.ps1
```

This script:

- Builds the Docker image  
- Pushes to DOCR  
- Applies Kustomize overlays  
- Restarts the deployment  

---

# 🛠️ **Deployment Troubleshooting Guide**

This is where you solve 90% of real-world issues fast.

---

## ❗ 1. App loads but JS bundle fails (MIME type error)

Cause: Ingress misrouting static files.

Fix:

- Ensure no rewrite-target annotation exists  
- Ensure `/shop` is defined in the main ingress  
- Restart ingress controller  

```
kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx
```

---

## ❗ 2. Domain works but www.domain doesn’t

Cause: DNS mismatch or stale ingress config.

Fix:

- Verify DNS A/CNAME records  
- Restart ingress controller  
- Ensure TLS hosts include both apex + www  

---

## ❗ 3. Redirects not working

Cause: Ingress precedence rules.

Fix:

- Ensure the main ingress defines the path  
- Ensure the redirect ingress uses permanent-redirect  
- Ensure both ingresses define the same hosts  

---

## ❗ 4. MongoDB connection fails

See the MongoDB troubleshooting section below.

---

## ❗ 5. Cert-manager not issuing certificates

Fix:

```
kubectl describe certificate -n wilkenweb-prod
kubectl describe challenge -n wilkenweb-prod
```

Common causes:

- DNS not pointing to ingress  
- Wrong issuer name  
- Missing ingress.class annotation  

---

# 🗺️ **Infrastructure Architecture Diagram (Text-Based)**

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
                │ (LoadBalancer)  │               │ (Let's Encrypt)  │
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

# 🍃 **MongoDB Connection Troubleshooting**

## 1. Check the connection string in Kubernetes

```
kubectl get secret mongo-connection -n wilkenweb-prod -o yaml
```

Ensure it contains:

```
mongodb+srv://...
```

or

```
mongodb://user:pass@mongo:27017/db?authSource=admin
```

---

## 2. Test connectivity from inside the pod

```
kubectl exec -it <pod> -n wilkenweb-prod -- bash
apt-get update && apt-get install -y curl
curl mongo:27017
```

If it fails → service or DNS issue.

---

## 3. Check MongoDB firewall rules

DigitalOcean must allow:

- VPC traffic  
- Kubernetes node pool CIDR  

---

## 4. Check authentication

If using DO Managed Mongo:

- authSource must be `admin`  
- username/password must match DO credentials  

---

# 🔄 **CI/CD Pipeline Plan**

Here’s a clean, modern pipeline you can implement in GitHub Actions or Azure DevOps.

---

## 🧱 Stages

### 1. **Build**
- Install Node + .NET  
- Build client (Vite)  
- Build server  
- Run tests  

### 2. **Docker Build**
- Build multi-stage Dockerfile  
- Tag with commit SHA  
- Push to DOCR  

### 3. **Deploy**
- Authenticate to Kubernetes  
- Apply Kustomize overlays  
- Restart deployment  

---

## 🧪 Optional Enhancements
- Canary deployments  
- Blue/green rollouts  
- Image scanning  
- Terraform plan/apply automation  

---

# 🔐 **Security Hardening**

### Ingress headers:

```
add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
add_header X-Frame-Options "DENY" always;
add_header X-Content-Type-Options "nosniff" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
```

### Other recommendations:
- Enable Pod Security Standards  
- Use read-only root filesystem  
- Add resource limits  
- Enable autoscaling  
- Add rate limiting at ingress  

---

# 📊 **Observability Roadmap**

### Phase 1 — Basic
- Enable NGINX access logs  
- Enable ASP.NET structured logging  
- Add log retention  

### Phase 2 — Metrics
- Install Prometheus  
- Install Grafana  
- Add dashboards for:
  - CPU/memory  
  - Ingress traffic  
  - Response times  
  - MongoDB metrics  

### Phase 3 — Tracing
- Add OpenTelemetry  
- Trace:
  - API calls  
  - DB queries  
  - Ingress latency  

---
