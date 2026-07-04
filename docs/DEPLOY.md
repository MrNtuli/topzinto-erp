# TE-ERP Production Deployment Guide

Deploy TopZinto ERP on a Linux VPS (Ubuntu 22.04+ recommended) with Docker, PostgreSQL, MinIO, Redis, and automatic HTTPS via Caddy + Let's Encrypt.

---

## 1. Server requirements

| Resource | Minimum |
|----------|---------|
| CPU | 2 vCPU |
| RAM | 4 GB |
| Disk | 40 GB SSD |
| OS | Ubuntu 22.04 LTS |

Open firewall ports **80** and **443** (HTTP/HTTPS). Do **not** expose Postgres, Redis, or MinIO publicly.

---

## 2. DNS

Point your domain to the server public IP:

```
A    erp.yourcompany.co.za    →  203.0.113.10
```

Wait for DNS propagation before starting Caddy (Let's Encrypt validates the domain).

---

## 3. Install Docker

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# log out and back in
docker compose version
```

---

## 4. Clone and configure

```bash
git clone <your-repo-url> topzinto-erp
cd topzinto-erp
```

### Environment file

```bash
cp deploy/.env.production.example .env.production
nano .env.production
```

Set strong values for `POSTGRES_PASSWORD`, `MINIO_ROOT_PASSWORD`, and `JWT_KEY` (32+ random characters).

### Caddyfile (HTTPS)

```bash
cp deploy/Caddyfile.production deploy/Caddyfile
nano deploy/Caddyfile
```

Replace `erp.example.co.za` with your real domain:

```
erp.yourcompany.co.za {
    reverse_proxy web:80
}
```

Caddy automatically obtains and renews a Let's Encrypt certificate. No manual certbot steps required.

---

## 5. Deploy

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build
```

Verify:

```bash
docker compose ps
curl -s https://erp.yourcompany.co.za/api/health
```

Expected health response includes `"cache":"redis"` when Redis is connected.

---

## 6. First login

Open `https://erp.yourcompany.co.za` and sign in with the seeded admin account, then **change the default password immediately** in Settings.

Default seed credentials (change after first login):

| Email | Password |
|-------|----------|
| admin@topzinto.com | Topzinto@2024 |

---

## 7. Backups

Daily PostgreSQL backups run automatically when `Backup:Enabled` is true (default in Docker).

- View and download backups in **Settings → Database Backups**
- Backups are stored in the API container `backups/` volume
- Retention: last 7 backups (configurable via `Backup__RetentionCount`)

For off-site backup, copy files from the API container periodically:

```bash
docker cp topzinto-api:/app/backups ./local-backups
```

---

## 8. Updates

```bash
git pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build
```

Database migrations apply automatically on API startup.

---

## 9. CI/CD (GitHub Actions)

The repository includes `.github/workflows/ci.yml` which runs on push/PR to `main`, `master`, or `develop`:

1. **Build API** — `dotnet build` (Release)
2. **Test API** — `dotnet test` (RBAC + health/auth smoke tests)
3. **Build Web** — `npm ci` + `npm run build`
4. **Docker build** — validates `Dockerfile.api` and `Dockerfile.web`

Push your code to GitHub to enable the pipeline. No secrets required for build-only CI.

### Production deploy workflow

`.github/workflows/deploy.yml` deploys to your VPS via SSH (manual trigger only — safe for first go-live).

**GitHub → Settings → Secrets and variables → Actions:**

| Secret | Example | Purpose |
|--------|---------|---------|
| `DEPLOY_HOST` | `203.0.113.10` | VPS IP or hostname |
| `DEPLOY_USER` | `ubuntu` | SSH username |
| `DEPLOY_SSH_KEY` | `(private key)` | PEM key for SSH auth |
| `DEPLOY_PATH` | `/opt/topzinto-erp` | Repo path on the server |

**Run a deploy:** GitHub → Actions → **Deploy Production** → Run workflow → choose branch (`main`).

The workflow runs `git pull` and `docker compose … up -d --build` on the server, then hits `/api/health`.

**On-server manual update** (same steps):

```bash
cd /opt/topzinto-erp
git pull
bash deploy/update.sh
```

**Go-live checklist:** [`docs/GO-LIVE.md`](GO-LIVE.md)

---

## 10. Troubleshooting

### Certificate not issued

- Confirm DNS A record points to this server
- Port 80 must be reachable (Let's Encrypt HTTP-01 challenge)
- Check Caddy logs: `docker logs topzinto-caddy`

### CORS errors in browser

Ensure `Cors__AllowedOrigins__0` in `.env.production` matches your HTTPS domain exactly (no trailing slash):

```
Cors__AllowedOrigins__0=https://erp.yourcompany.co.za
```

### SignalR / chat disconnects

Caddy and nginx are preconfigured for WebSocket upgrade on `/hubs/`. If using a custom reverse proxy, enable WebSocket passthrough.

### API not reachable

In production overlay, port 5000 is not exposed. All traffic goes through Caddy → nginx → API.

---

## 12. Email notifications (optional)

Chat @mention alerts can be emailed when SMTP is configured.

In `.env.production`:

```
Email__Enabled=true
Email__FromAddress=noreply@yourcompany.co.za
Email__Smtp__Host=smtp.yourprovider.com
Email__Smtp__Port=587
Email__Smtp__UseSsl=true
Email__Smtp__Username=your-smtp-user
Email__Smtp__Password=your-smtp-password
App__BaseUrl=https://erp.yourcompany.co.za
```

When disabled (default), mention emails are skipped and a log line is written instead — in-app notifications still work.

Verify: `GET /api/health` should show `"email":"smtp"` when enabled.

---

## 11. Local HTTPS (development)

For local self-signed HTTPS testing:

```powershell
docker compose --profile ssl up -d --build
# https://localhost (browser will warn about self-signed cert)
```

This uses `tls internal` in `deploy/Caddyfile` — not suitable for production.
