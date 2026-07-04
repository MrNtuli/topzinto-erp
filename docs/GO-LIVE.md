# TE-ERP Go-Live Checklist

Use this before pointing TopZinto staff at the production URL.

---

## 1. Server & DNS

- [ ] VPS provisioned (Ubuntu 22.04+, 2 vCPU, 4 GB RAM)
- [ ] Docker + Docker Compose installed
- [ ] Firewall: ports **80** and **443** open only
- [ ] DNS A record points to VPS (`erp.yourcompany.co.za`)

---

## 2. Configuration

- [ ] Repo cloned on server (`DEPLOY_PATH`, e.g. `/opt/topzinto-erp`)
- [ ] `.env.production` created from `deploy/.env.production.example`
- [ ] Strong `POSTGRES_PASSWORD`, `MINIO_ROOT_PASSWORD`, `JWT_KEY` (32+ chars)
- [ ] `deploy/Caddyfile` copied from `deploy/Caddyfile.production` with real domain
- [ ] `Cors__AllowedOrigins__0` matches HTTPS domain exactly
- [ ] SMTP configured if email alerts are required (`Email__Enabled=true`)

---

## 3. First deploy

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build
curl -s https://erp.yourcompany.co.za/api/health
```

Expected: `"status":"healthy"`, `"cache":"redis"`.

---

## 4. Security (day one)

- [ ] Log in as `admin@topzinto.com` / `Topzinto@2024`
- [ ] **Change admin password** immediately (Settings)
- [ ] Create real user accounts for each staff member
- [ ] Deactivate or remove test accounts
- [ ] Confirm RBAC roles match job titles (Settings → Manage Users)

---

## 5. GitHub CI/CD (optional)

- [ ] Repo pushed to GitHub (see [`GITHUB.md`](GITHUB.md))
- [ ] GitHub secrets set (see [`DEPLOY.md`](DEPLOY.md) §9)
- [ ] CI pipeline green on `main`
- [ ] Manual deploy tested: Actions → **Deploy Production** → Run workflow

---

## 6. Backups & monitoring

- [ ] Settings → **Create Backup Now** works
- [ ] Off-site backup plan documented (weekly `docker cp` or VPS snapshots)
- [ ] Directors receive system alert emails (if SMTP on)

---

## 7. User onboarding

- [ ] Share production URL and login instructions
- [ ] Brief walkthrough: dashboard, projects, site reports, notifications
- [ ] Support contact defined for login issues

---

## Done?

When all boxes are checked, TE-ERP is **live for internal use**. Future work (Sage, payroll, client portals) is v3+.
