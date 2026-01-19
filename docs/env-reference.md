# 🔐 Environment Variable Reference  
*A complete reference for all environment variables used across development, staging, and production.*

This document explains:

- What each variable does  
- Where it is used  
- Whether it is required or optional  
- Differences between dev, staging, and production  
- Security considerations  
- Example `.env` files  

Environment variables are the backbone of configuration for this platform.  
They ensure that the same codebase can run in multiple environments with different settings.

---

# 🧱 1. Overview

The platform uses environment variables for:

- MongoDB connection  
- Stripe payments  
- Printful fulfillment  
- SMTP email  
- API base URLs  
- Server configuration  
- Client configuration  
- Third‑party integrations  

Environment variables are loaded:

- **Locally** via `.env`  
- **In Kubernetes** via Secrets  
- **In CI/CD** via pipeline variables  

---

# 🧪 2. Environment Types

The platform supports three environment tiers:

### **Development**
- Local Podman MongoDB  
- Local Vite dev server  
- Local ASP.NET server  
- Debug logging enabled  

### **Staging** (optional future tier)
- Hosted MongoDB  
- Hosted Kubernetes  
- Test Stripe keys  
- Test Printful keys  

### **Production**
- DigitalOcean Managed MongoDB  
- Production Stripe keys  
- Production Printful keys  
- SMTP email enabled  
- TLS enforced  

---

# 📄 3. Full Variable Reference

Below is a complete list of all environment variables used across the platform.

---

## 🟦 Application & Server

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `ASPNETCORE_URLS` | Yes | All | Controls which URLs the server binds to. In dev, overridden by script. |
| `SERVER_PROXY_PORT` | Yes | Dev | Port Vite uses to proxy API requests. |
| `VITE_API_BASE_URL` | Yes | Dev | Base URL for API calls from the client. |

---

## 🟩 MongoDB

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `DATABASE_URL` | Yes | All | Full MongoDB connection string used by the server. |
| `MONGO_INITDB_DATABASE` | Yes | Dev | Database name for local MongoDB initialization. |
| `MONGO_INITDB_ROOT_USERNAME` | Yes | Dev | Root username for local MongoDB. |
| `MONGO_INITDB_ROOT_PASSWORD` | Yes | Dev | Root password for local MongoDB. |
| `MONGO_CONNECTION_STRING` | Optional | Dev | Used by Podman Compose for local DB. |

### Notes
- In production, `DATABASE_URL` comes from DigitalOcean Managed MongoDB.  
- In dev, it points to `localhost:27017`.

---

## 🟧 Stripe Payments

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `VITE_STRIPE_API_PK` | Yes | All | Public Stripe key used by the client. |
| `STRIPE_API_SK` | Yes | All | Secret Stripe key used by the server. |

### Notes
- Dev uses test keys.  
- Production uses live keys.  
- Never commit these keys to Git.

---

## 🟨 Printful Integration

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `PRINTFUL_API_KEY` | Yes | All | API key for Printful fulfillment. |
| `PRINTFUL_OAUTH_KEY` | Optional | All | OAuth key if using OAuth flow. |
| `PRINTFUL_STORE_ID` | Yes | All | Store ID for Printful shop. |

---

## 🟫 Email (SMTP)

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `GMAIL_SMTP_HOST` | Yes | Prod | SMTP host for sending emails. |
| `GMAIL_SMTP_PORT` | Yes | Prod | SMTP port (587). |
| `GMAIL_USERNAME` | Yes | Prod | Gmail username. |
| `GMAIL_APP_PASSWORD` | Yes | Prod | App password for Gmail SMTP. |
| `GMAIL_FROM_NAME` | Yes | Prod | Display name for outgoing emails. |
| `GMAIL_FROM_ADDRESS` | Yes | Prod | From address for outgoing emails. |

### Notes
- SMTP is optional in dev.  
- In production, Gmail requires an **App Password**, not the account password.

---

## 🟪 Elasticsearch (Optional Future Feature)

| Variable | Required | Environment | Description |
|---------|----------|-------------|-------------|
| `ELASTICSEARCH_URL` | Optional | Dev | URL for local Elasticsearch instance. |

---

# 🧾 4. Example `.env` Files

---

## 🟦 Development `.env`

```
VITE_STRIPE_API_PK=pk_test_123
STRIPE_API_SK=sk_test_123

PRINTFUL_API_KEY=pf_test_123
PRINTFUL_OAUTH_KEY=pf_oauth_123
PRINTFUL_STORE_ID=12345

DATABASE_URL=mongodb://xero:dev123@localhost:27017/xeroeffort?authSource=xeroeffort
MONGO_INITDB_DATABASE=xeroeffort
MONGO_INITDB_ROOT_USERNAME=xero
MONGO_INITDB_ROOT_PASSWORD=dev123

ELASTICSEARCH_URL=http://localhost:9200

ASPNETCORE_URLS=http://0.0.0.0:5000
VITE_API_BASE_URL=localhost
SERVER_PROXY_PORT=5000

GMAIL_SMTP_HOST=smtp.gmail.com
GMAIL_SMTP_PORT=587
GMAIL_USERNAME=xeroeffortclub@gmail.com
GMAIL_APP_PASSWORD=dev-only
GMAIL_FROM_NAME=Xero Effort
GMAIL_FROM_ADDRESS=xeroeffortclub@gmail.com
```

---

## 🟧 Staging `.env` (future)

```
VITE_STRIPE_API_PK=pk_test_staging
STRIPE_API_SK=sk_test_staging

PRINTFUL_API_KEY=pf_test_staging
PRINTFUL_STORE_ID=12345

DATABASE_URL=mongodb+srv://staging-user:pass@cluster.mongodb.net/staging
ASPNETCORE_URLS=http://0.0.0.0:5000
```

---

## 🟥 Production `.env`

```
VITE_STRIPE_API_PK=pk_live_...
STRIPE_API_SK=sk_live_...

PRINTFUL_API_KEY=pf_live_...
PRINTFUL_STORE_ID=12345

DATABASE_URL=mongodb+srv://prod-user:pass@cluster.mongodb.net/prod

ASPNETCORE_URLS=http://0.0.0.0:5000
VITE_API_BASE_URL=https://seanwilken.com

GMAIL_SMTP_HOST=smtp.gmail.com
GMAIL_SMTP_PORT=587
GMAIL_USERNAME=xeroeffortclub@gmail.com
GMAIL_APP_PASSWORD=xxxx
GMAIL_FROM_NAME=Xero Effort
GMAIL_FROM_ADDRESS=xeroeffortclub@gmail.com
```

---

# 🛡️ 5. Security Best Practices

- Never commit `.env` files to Git  
- Use Kubernetes Secrets in production  
- Rotate API keys regularly  
- Use different keys for dev/staging/prod  
- Use Gmail App Passwords, not real passwords  
- Restrict MongoDB access to private networks  

---

# 🎉 Summary

This reference gives you:

- A complete map of all environment variables  
- Clear explanations of what each one does  
- Example `.env` files for every environment  
- Security guidance  
- A foundation for future contributors  

Environment variables are the backbone of configuration — and now they’re fully documented.
