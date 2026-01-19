# 🚨 Production Incident Runbook  
*A practical, battle‑tested guide for diagnosing and resolving production issues on the SeanWilkenWeb platform.*

This runbook is designed for fast, reliable action during incidents.  
It provides:

- A structured troubleshooting flow  
- Commands for diagnosis  
- Common failure scenarios  
- Recovery procedures  
- Escalation guidance  

Use this document when something goes wrong in production.

---

# 🧭 1. Incident Response Philosophy

When something breaks:

1. **Stay calm**  
2. **Follow the checklist**  
3. **Gather data before acting**  
4. **Fix the root cause, not the symptom**  
5. **Document what happened**  

This runbook is designed to reduce panic and increase clarity.

---

# 🟥 2. High‑Level Incident Flow

```
1. Identify the symptom
2. Check Kubernetes health
3. Check ingress + routing
4. Check TLS + certificates
5. Check application logs
6. Check MongoDB connectivity
7. Check DNS
8. Roll back or restart if needed
9. Document the incident
```

---

# 🟦 3. Quick Commands Cheat Sheet

### Get pods
```
kubectl get pods -n wilkenweb-prod
```

### Describe a pod
```
kubectl describe pod <pod> -n wilkenweb-prod
```

### View logs
```
kubectl logs <pod> -n wilkenweb-prod
```

### Restart deployment
```
kubectl rollout restart deployment app -n wilkenweb-prod
```

### Check ingress
```
kubectl get ingress -n wilkenweb-prod
kubectl describe ingress app-ingress -n wilkenweb-prod
```

### Check certificate
```
kubectl describe certificate -n wilkenweb-prod
kubectl describe challenge -n wilkenweb-prod
```

---

# 🟩 4. Common Incident Scenarios & Fixes

---

## ❗ Scenario A: Site is down (502/503)

### Likely Causes
- App pods crashed  
- Deployment failed  
- MongoDB unreachable  
- Ingress misrouting  

### Steps
1. Check pods:
   ```
   kubectl get pods -n wilkenweb-prod
   ```
2. If pods are CrashLoopBackOff:
   ```
   kubectl logs <pod> -n wilkenweb-prod
   ```
3. Restart deployment:
   ```
   kubectl rollout restart deployment app -n wilkenweb-prod
   ```

---

## ❗ Scenario B: JS bundle fails to load (MIME type error)

### Cause
Ingress routing issue — static files being served as HTML.

### Fix
1. Ensure no rewrite-target annotation exists  
2. Ensure `/shop` is defined in main ingress  
3. Restart ingress controller:
   ```
   kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx
   ```

---

## ❗ Scenario C: Domain works but www.domain doesn’t

### Cause
- DNS mismatch  
- Stale ingress config  
- Missing TLS host entry  

### Fix
1. Check DNS  
2. Restart ingress controller  
3. Verify TLS hosts include both apex + www  

---

## ❗ Scenario D: TLS certificate invalid or expired

### Steps
1. Check certificate:
   ```
   kubectl describe certificate -n wilkenweb-prod
   ```
2. Check ACME challenge:
   ```
   kubectl describe challenge -n wilkenweb-prod
   ```
3. Ensure DNS points to ingress  
4. Ensure ingress has:
   ```
   cert-manager.io/cluster-issuer: letsencrypt-prod
   ```

---

## ❗ Scenario E: MongoDB connection failing

### Symptoms
- App logs show connection errors  
- API endpoints fail  
- Site loads but dynamic data missing  

### Steps
1. Check logs:
   ```
   kubectl logs <pod> -n wilkenweb-prod
   ```
2. Test connectivity from inside pod:
   ```
   kubectl exec -it <pod> -n wilkenweb-prod -- bash
   curl mongo:27017
   ```
3. Verify secret:
   ```
   kubectl get secret mongo-connection -n wilkenweb-prod -o yaml
   ```
4. Check DO firewall rules  
5. Restart app:
   ```
   kubectl rollout restart deployment app -n wilkenweb-prod
   ```

---

## ❗ Scenario F: Redirects not working

### Cause
Ingress precedence issue.

### Fix
- Ensure main ingress defines `/shop`  
- Ensure redirect ingress uses permanent-redirect  
- Ensure both ingresses define same hosts  

---

# 🟨 5. Deep Diagnostics

---

## 🔍 Check Pod Events
```
kubectl describe pod <pod> -n wilkenweb-prod
```

Look for:
- Image pull errors  
- CrashLoopBackOff  
- OOMKilled  
- Readiness probe failures  

---

## 🔍 Check Ingress Events
```
kubectl describe ingress app-ingress -n wilkenweb-prod
```

Look for:
- TLS errors  
- Challenge failures  
- Backend service issues  

---

## 🔍 Check Deployment Status
```
kubectl rollout status deployment app -n wilkenweb-prod
```

---

## 🔍 Check Service Routing
```
kubectl get svc -n wilkenweb-prod
kubectl describe svc app -n wilkenweb-prod
```

---

## 🔍 Check DNS Resolution
From your machine:
```
nslookup seanwilken.com
nslookup www.seanwilken.com
```

Both must point to the ingress LoadBalancer IP.

---

# 🟧 6. Recovery Procedures

---

## 🔄 Restart the Application
```
kubectl rollout restart deployment app -n wilkenweb-prod
```

---

## 🔄 Restart Ingress Controller
```
kubectl rollout restart deployment ingress-nginx-controller -n ingress-nginx
```

---

## 🔄 Roll Back Deployment
If a deployment is bad:

```
kubectl rollout undo deployment app -n wilkenweb-prod
```

---

## 🔄 Reapply Ingress
```
kubectl apply -k infrastructure/k8s/overlays/production
```

---

# 🟥 7. Escalation Guidelines

Escalate if:

- TLS fails repeatedly  
- MongoDB is unreachable for >10 minutes  
- Ingress routing breaks across all domains  
- Deployment rollback does not fix the issue  
- DNS propagation issues persist beyond expected windows  

Document:

- What happened  
- What you tried  
- What fixed it  
- What needs improvement  

---

# 🟦 8. Post‑Incident Review Template

```
## Summary
What happened?

## Impact
Who/what was affected?

## Root Cause
What caused the issue?

## Resolution
How was it fixed?

## Timeline
- 00:00 — Issue detected
- 00:05 — Investigation started
- 00:15 — Root cause identified
- 00:20 — Fix applied
- 00:25 — System stable

## Action Items
- [ ] Improve monitoring
- [ ] Add documentation
- [ ] Add automation
```

---

# 🎉 Final Notes

This runbook is designed to be:

- Fast  
- Practical  
- Actionable  
- Stress‑reducing  
- Easy to follow  

When something breaks, this is your guide.
