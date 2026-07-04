# Push TE-ERP to GitHub

One-time setup to enable **CI** (build + test on every push) and **Deploy Production** (manual VPS deploy).

---

## 1. Install Git

Download: https://git-scm.com/download/win

Verify:

```powershell
git --version
```

---

## 2. Initialize the repository (local)

From the project root:

```powershell
cd "c:\Users\Lenovo\Documents\PROJECTS\TOPZINTO\Topzinto System"
git init
git branch -M main
```

Check what will be tracked (no `.env`, no `topzinto_erp.db`, no `node_modules`):

```powershell
git status
```

---

## 3. First commit

```powershell
git add .
git status
git commit -m "Initial commit — TopZinto TE-ERP v2.27"
```

Review `git status` before commit — ensure no secrets appear in staged files.

---

## 4. Create GitHub repository

1. Go to https://github.com/new
2. Name: `topzinto-erp` (or your choice)
3. **Private** recommended (construction business data)
4. Do **not** add README, .gitignore, or license (already in repo)
5. Create repository

---

## 5. Push to GitHub

Replace `YOUR_USER` and `YOUR_REPO`:

```powershell
git remote add origin https://github.com/YOUR_USER/YOUR_REPO.git
git push -u origin main
```

Use a **Personal Access Token** as password if prompted (GitHub → Settings → Developer settings → Tokens).

---

## 6. Verify CI

After push, open GitHub → **Actions** → **CI** workflow should run:

- Build API + `dotnet test` (20 tests)
- Build Web
- Docker build

All green = ready for team development.

---

## 7. Enable production deploy (optional)

When VPS is ready, add **Repository secrets** (Settings → Secrets → Actions):

| Secret | Value |
|--------|-------|
| `DEPLOY_HOST` | VPS IP |
| `DEPLOY_USER` | e.g. `ubuntu` |
| `DEPLOY_SSH_KEY` | Private SSH key (full PEM) |
| `DEPLOY_PATH` | e.g. `/opt/topzinto-erp` |

Deploy: Actions → **Deploy Production** → Run workflow.

See also: [`DEPLOY.md`](DEPLOY.md), [`GO-LIVE.md`](GO-LIVE.md)

---

## Daily workflow

```powershell
git pull
# ... make changes ...
git add .
git commit -m "Describe your change"
git push
```

CI runs automatically on push to `main`.
