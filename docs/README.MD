# 📚 SeanWilkenWeb Documentation

Welcome to the documentation hub for the **SeanWilkenWeb** and **XeroEffort** platform.  
This directory contains all technical, architectural, operational, and onboarding documentation for the project.

The goal of this documentation suite is to provide:

- A clear understanding of how the platform works  
- A smooth onboarding experience for new developers  
- A reference for day‑to‑day development and operations  
- A foundation for future blog posts and public documentation  
- A maintainable, extensible knowledge base as the platform evolves  

---

# 🧭 Documentation Index

## 🏗️ Architecture & Design
- [Architecture Overview](architecture.md)  
- [Philosophy of the Platform](philosophy.md)

## 🚀 Getting Started
- [Developer Onboarding Checklist](onboarding.md)  
- [Development Scripts Guide](dev-scripts.md)  
- [Environment Variable Reference](env-reference.md)

## 🔄 CI/CD & Deployment
- [CI/CD Pipeline](cicd.md)  
- [End‑to‑End Deployment Walkthrough](walkthroughs/deployment-end-to-end.md)

## 🛡️ Operations & Reliability
- [Production Incident Runbook](runbook.md)  
- [MongoDB Connection Guide](walkthroughs/mongo-connection.md)  
- [TLS & Certificate Management](walkthroughs/tls-certificates.md)

## 🌐 Networking & Routing
- [Ingress Routing Deep Dive](walkthroughs/ingress-routing.md)

## 🐳 Build & Runtime
- [Dockerfile Deep Dive](walkthroughs/dockerfile-deep-dive.md)  
- [Vite Build Pipeline](walkthroughs/vite-build-pipeline.md)

---

# 📘 Cheat Sheets

## Kubernetes
- [Kubernetes Cheat Sheet](cheat-sheets/kubernetes-cheatsheet.md)

## Ingress & Networking
- [Ingress Cheat Sheet](cheat-sheets/ingress-cheatsheet.md)

## Docker
- [Docker Cheat Sheet](cheat-sheets/docker-cheatsheet.md)

## MongoDB
- [MongoDB Cheat Sheet](cheat-sheets/mongo-cheatsheet.md)

## Debugging
- [Debugging Cheat Sheet](cheat-sheets/debugging-cheatsheet.md)

---

# 🧵 Walkthroughs (Deep Dives)

These are narrative, step‑by‑step explanations of how key parts of the system work.

- [Ingress Routing](walkthroughs/ingress-routing.md)  
- [TLS Certificates](walkthroughs/tls-certificates.md)  
- [Dockerfile Deep Dive](walkthroughs/dockerfile-deep-dive.md)  
- [Vite Build Pipeline](walkthroughs/vite-build-pipeline.md)  
- [MongoDB Connection](walkthroughs/mongo-connection.md)  
- [Deployment End‑to‑End](walkthroughs/deployment-end-to-end.md)

---

# 🧱 How to Use This Documentation

### If you're new to the project:
Start with:
1. **Developer Onboarding Checklist**  
2. **Architecture Overview**  
3. **Development Scripts Guide**

### If you're debugging:
Jump to:
- **Runbook**  
- **Debugging Cheat Sheet**  
- **Ingress Cheat Sheet**

### If you're deploying:
Use:
- **CI/CD Pipeline**  
- **Deployment Walkthrough**

### If you're writing blog posts:
Use:
- **Philosophy of the Platform**  
- **Architecture Overview**  
- **Walkthroughs**

---

# 📝 Contributing to the Docs

Documentation is a living part of the platform.  
If you add features or change infrastructure, update the relevant doc or create a new one.

Guidelines:
- Keep documents focused  
- Use clear headings  
- Prefer examples over theory  
- Link related docs together  
- Keep tone consistent (professional + personal insight)

---

# Development Environment Configuration

infrastructure/docker/App/Development/.env

```bash

VITE_STRIPE_API_PK=pk_test_...
STRIPE_API_SK=sk_test_...
PRINTFUL_API_KEY=...
PRINTFUL_OAUTH_KEY=...
MONGO_INITDB_DATABASE=xeroeffort
MONGO_INITDB_ROOT_USERNAME=xero
MONGO_INITDB_ROOT_PASSWORD=dev123
MONGO_CONNECTION_STRING=mongodb://xero:dev123@localhost:27017/
ELASTICSEARCH_URL=http://localhost:9200
ASPNETCORE_URLS=http://0.0.0.0:5000
VITE_API_BASE_URL=localhost
SERVER_PROXY_PORT=5000
PRINTFUL_STORE_ID=...
GMAIL_SMTP_HOST=smtp.gmail.com
GMAIL_SMTP_PORT=587
GMAIL_USERNAME=mail@gmail.com
GMAIL_APP_PASSWORD=...
GMAIL_FROM_NAME=Xero Effort
GMAIL_FROM_ADDRESS=mail@gmail.com

```



# 🎉 Final Notes

This documentation suite is designed to scale with the platform.  
As the project grows — new services, new domains, new pipelines — the docs folder will grow with it.

Welcome aboard, and enjoy exploring the system.


