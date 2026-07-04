#!/usr/bin/env bash
# Run on the VPS from the repo root after git pull.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ ! -f .env.production ]]; then
  echo "Missing .env.production — copy from deploy/.env.production.example"
  exit 1
fi

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build
docker compose ps

echo "Health check:"
sleep 10
curl -sf http://localhost/api/health || curl -sf "https://${APP_DOMAIN:-localhost}/api/health" || true
echo ""
