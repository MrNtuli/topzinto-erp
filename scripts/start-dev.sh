#!/usr/bin/env bash
# Starts TopZinto TE-ERP API + Web dev servers in separate terminals.
# Run from repo root: ./scripts/start-dev.sh

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
API_DIR="$ROOT/src/Topzinto.Erp.Api"
WEB_DIR="$ROOT/src/Topzinto.Erp.Web"

echo ""
echo "TopZinto TE-ERP — starting dev stack (v2.32)"
echo "============================================="
echo ""

launch_terminal() {
  local title="$1"
  local dir="$2"
  local cmd="$3"

  if command -v gnome-terminal >/dev/null 2>&1; then
    gnome-terminal --title="$title" -- bash -lc "cd '$dir' && $cmd; exec bash"
  elif command -v xterm >/dev/null 2>&1; then
    xterm -T "$title" -e bash -lc "cd '$dir' && $cmd; exec bash" &
  elif [[ "$OSTYPE" == "darwin"* ]]; then
    osascript -e "tell application \"Terminal\" to do script \"cd '$dir' && $cmd\""
  else
    echo "Could not detect a terminal emulator. Run manually:"
    echo "  cd '$dir' && $cmd"
  fi
}

echo "Launching API  -> http://localhost:5000"
launch_terminal "TopZinto API" "$API_DIR" "dotnet run"

sleep 2

echo "Launching Web  -> http://localhost:5173"
launch_terminal "TopZinto Web" "$WEB_DIR" "npm run dev"

echo ""
echo "Waiting for API health check..."

healthy=false
for _ in $(seq 1 30); do
  if response="$(curl -sf http://localhost:5000/api/health 2>/dev/null)"; then
    if echo "$response" | grep -q '"status"[[:space:]]*:[[:space:]]*"healthy"'; then
      healthy=true
      version="$(echo "$response" | sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p')"
      break
    fi
  fi
  sleep 2
done

echo ""
if [[ "$healthy" == true ]]; then
  echo "API is healthy (version ${version:-unknown})"
else
  echo "API not ready yet — check the API terminal window."
fi

echo ""
echo "Open the app:  http://localhost:5173"
echo "Swagger (dev): http://localhost:5000/swagger"
echo "Login:         admin@topzinto.com / Topzinto@2024"
echo ""
echo "Tip: Use 'Remember me' for a 7-day session, or leave unchecked for an 8-hour browser session."
echo ""
