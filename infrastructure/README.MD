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




