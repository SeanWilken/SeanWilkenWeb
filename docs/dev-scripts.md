# 🛠️ Development Scripts & Local Environment Guide  
*A complete guide to the local development workflow, Podman Compose stack, environment loader, and dev automation scripts.*

This document explains how the development environment works, how to use the provided scripts, and how to customize or extend them for future contributors.

---

# 📘 Overview

The local development environment is designed to:

- Start MongoDB automatically  
- Load environment variables from `.env`  
- Run the backend with `dotnet watch`  
- Run the frontend with Vite + Fable  
- Clean up everything on exit  
- Require **one command** to start:

```
./dev.ps1
```

This script orchestrates the entire dev stack.

---

# 🧱 Directory Structure

```
infrastructure/
└── docker/
    └── App/
        └── Development/
            ├── docker-compose.dev.yml
            ├── .env.example
            └── .env
```

The dev script lives at the root of the repo:

```
dev.ps1
```

---

# 🐳 1. Podman Compose Dev Stack

The development environment uses **Podman Compose** instead of Docker Compose.

### Why Podman?
- Rootless containers  
- Better security model  
- Fully Docker‑compatible  
- Works on Windows, macOS, Linux  

### Services Started
The dev compose file currently starts:

- **MongoDB**  
  - Exposed on `localhost:27017`  
  - Initialized with credentials from `.env`  

### Example Compose File

```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:latest
    environment:
      MONGO_CONNECTION_STRING: ${MONGO_CONNECTION_STRING}
      MONGO_INITDB_ROOT_USERNAME: ${MONGO_INITDB_ROOT_USERNAME}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_INITDB_ROOT_PASSWORD}
    ports:
      - "27017:27017"
    deploy:
      resources:
        limits:
          cpus: "1.0"
          memory: 1g
```

---

# 🔐 2. Environment Variable Loader

The script loads environment variables from:

```
infrastructure/docker/App/Development/.env
```

### Supported Features
- Ignores comments  
- Ignores empty lines  
- Loads all key/value pairs  
- Skips `ASPNETCORE_URLS` (controlled manually)  
- Makes variables available to both server and client  

### Example `.env` file

```
VITE_STRIPE_API_PK=pk_test_...
STRIPE_API_SK=sk_test_...
PRINTFUL_API_KEY=...
DATABASE_URL=mongodb://xero:dev123@localhost:27017/xeroeffort?authSource=xeroeffort
MONGO_INITDB_DATABASE=xeroeffort
MONGO_INITDB_ROOT_USERNAME=xero
MONGO_INITDB_ROOT_PASSWORD=dev123
ASPNETCORE_URLS=http://0.0.0.0:5000
VITE_API_BASE_URL=localhost
SERVER_PROXY_PORT=5000
```

Full reference:  
👉 `docs/env-reference.md`

---

# ⚙️ 3. Backend: dotnet watch

The script starts the backend using:

```
dotnet watch run --urls http://localhost:5000
```

### Features
- Auto‑reload on file changes  
- Fast incremental builds  
- Runs on port 5000  
- Works seamlessly with Vite proxy  

---

# ⚡ 4. Frontend: Vite + Fable Dev Server

The script starts the client using:

```
dotnet fable watch -o output -s --run "pnpm exec vite"
```

### Features
- Hot module reload  
- Fable → JS compilation  
- Vite dev server on port 5173  
- Proxies `/api/*` to backend  

---

# 🔄 5. Full Script Flow (Step‑by‑Step)

Here is what `dev.ps1` does:

```
1. Resolve repo paths
2. Validate docker-compose.dev.yml exists
3. Start MongoDB via Podman Compose
4. Load environment variables from .env
5. Start backend (dotnet watch)
6. Start frontend (Vite + Fable)
7. On exit:
   - Stop backend process
   - Stop Podman containers
   - Return to repo root
```

---

# 🗺️ ASCII Diagram: Local Dev Architecture

```
┌──────────────────────────┐
│        dev.ps1           │
│  (Orchestrator Script)   │
└──────────────┬───────────┘
               │
               ▼
     ┌───────────────────┐
     │ Podman Compose    │
     │  - MongoDB        │
     └─────────┬─────────┘
               │
               ▼
     ┌───────────────────┐
     │ dotnet watch      │
     │  Backend API      │
     └─────────┬─────────┘
               │
               ▼
     ┌───────────────────┐
     │ Vite + Fable      │
     │  Frontend SPA     │
     └───────────────────┘
```

---

# 🧩 6. Customizing the Dev Environment

You can extend the dev stack by editing:

```
infrastructure/docker/App/Development/docker-compose.dev.yml
```

Examples:

### Add Redis
```yaml
redis:
  image: redis:latest
  ports:
    - "6379:6379"
```

### Add Elasticsearch
```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.0.0
  ports:
    - "9200:9200"
```

### Add a local SMTP server
```yaml
mailhog:
  image: mailhog/mailhog
  ports:
    - "8025:8025"
```

---

# 🧪 7. Troubleshooting the Dev Environment

### MongoDB won’t start
- Check if port 27017 is already in use  
- Delete old Podman volumes  
- Ensure `.env` has valid credentials  

### Backend won’t start
- Run `dotnet restore`  
- Ensure ASPNETCORE_URLS is not overridden  

### Frontend won’t start
- Run `pnpm install`  
- Ensure Node version matches `.nvmrc`  

### Environment variables not loading
- Ensure `.env` exists  
- Ensure no BOM characters  
- Ensure no trailing spaces  

---

# 🔮 8. Future Enhancements

- Add hot‑reload for server + client logs  
- Add optional Redis for caching  
- Add optional Mailhog for email testing  
- Add script flags:
  - `./dev.ps1 --no-client`
  - `./dev.ps1 --no-server`
  - `./dev.ps1 --no-db`  
- Add cross‑platform Bash version  

---

# 🎉 Summary

The development environment is designed to be:

- Fast  
- Simple  
- Reproducible  
- Extensible  
- One‑command friendly  

This script is the backbone of local development and will continue to evolve as the platform grows.
