# 🧭 Developer Onboarding Checklist  
*A complete, step‑by‑step guide for getting productive on the SeanWilkenWeb platform.*

This guide walks new developers through everything required to set up their environment, run the application locally, understand the infrastructure, and deploy changes confidently.

---

# 📘 1. Workstation Setup

Before doing anything else, install the required tools.

## Required Software
- **.NET SDK (8.0+)**
- **Node.js (LTS)**
- **PNPM**
- **Docker Desktop or Podman Desktop**
- **kubectl**
- **Terraform**
- **Git**
- **PowerShell 7+** (for dev scripts)

## Optional but Recommended
- **k9s** or **Lens** (Kubernetes UI)
- **MongoDB Compass**
- **Postman / Bruno**
- **DigitalOcean CLI**

---

# 📁 2. Clone the Repository

```
git clone https://github.com/<your-org>/<your-repo>.git
cd <your-repo>
```

---

# 🔧 3. Configure Environment Variables

Environment variables are loaded from `.env` files.

Copy the example file:

```
cp infrastructure/docker/App/Development/.env.example infrastructure/docker/App/Development/.env
```

Then update values for:

- MongoDB connection  
- Stripe keys  
- Printful keys  
- SMTP credentials  
- API base URLs  

Full reference:  
👉 `docs/env-reference.md`

---

# 🐳 4. Start the Local Development Stack

The easiest way to run the full stack locally is using the dev script:

```
./dev.ps1
```

This script:

- Starts MongoDB via Podman Compose  
- Loads environment variables  
- Runs the server via `dotnet watch`  
- Runs the client via Vite + Fable  
- Cleans up everything on exit  

Full explanation:  
👉 `docs/dev-scripts.md`

---

# 🧪 5. Verify Local Development

Once the dev stack is running:

### Client  
Open:  
```
http://localhost:5173
```

### Server  
Open:  
```
http://localhost:5000/api/health
```

### MongoDB  
Connect via Compass:  
```
mongodb://xero:dev123@localhost:27017/xeroeffort?authSource=xeroeffort
```

---

# 🧱 6. Understand the Architecture

Before contributing, review:

- **Architecture Overview**  
- **Ingress Routing**  
- **Dockerfile Deep Dive**  
- **Vite Build Pipeline**  
- **MongoDB Connection Guide**

These documents explain how the system works end‑to‑end.

Start here:  
👉 `docs/architecture.md`

---

# 🚢 7. Build the Production Image (Optional)

If you want to test the production build locally:

```
./scripts/build.ps1
```

This:

- Builds the client  
- Builds the server  
- Produces a multi‑stage Docker image identical to production  

---

# ☸️ 8. Connect to Kubernetes (Production/Staging)

If you have access to the cluster:

```
./scripts/kubeconfig-setup.ps1
kubectl get pods -n wilkenweb-prod
```

You should see:

```
app-xxxxx   Running
```

---

# 🚀 9. Deploy to Production

Deployment is handled via:

```
./scripts/deploy.ps1
```

This script:

- Builds the Docker image  
- Pushes to DOCR  
- Applies Kustomize overlays  
- Restarts the deployment  

Full CI/CD explanation:  
👉 `docs/cicd.md`

---

# 🛠️ 10. Troubleshooting

If something goes wrong:

- **Runbook** → operational issues  
- **Debugging Cheat Sheet** → quick fixes  
- **Ingress Cheat Sheet** → routing issues  
- **MongoDB Cheat Sheet** → DB issues  

Start here:  
👉 `docs/runbook.md`

---

# 🧠 11. Recommended Reading Order

For new developers:

1. **Onboarding Checklist (this file)**  
2. **Architecture Overview**  
3. **Dev Scripts Guide**  
4. **Ingress Routing Deep Dive**  
5. **Dockerfile Deep Dive**  
6. **Vite Build Pipeline**  
7. **CI/CD Pipeline**  
8. **Runbook**  

---

# 🎉 Welcome to the Platform

You now have everything you need to:

- Run the app locally  
- Understand the architecture  
- Deploy changes  
- Troubleshoot issues  
- Contribute confidently  

If you get stuck, check the docs or reach out to the platform maintainer.
