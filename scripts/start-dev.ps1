# Starts TopZinto TE-ERP API + Web dev servers in separate terminal windows.
# Run from repo root: .\scripts\start-dev.ps1

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$ApiDir = Join-Path $Root "src\Topzinto.Erp.Api"
$WebDir = Join-Path $Root "src\Topzinto.Erp.Web"

Write-Host ""
Write-Host "TopZinto TE-ERP — starting dev stack (v2.32)" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Launching API  -> http://localhost:5000" -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$ApiDir'; Write-Host 'TopZinto API (dotnet run)' -ForegroundColor Green; dotnet run"
)

Start-Sleep -Seconds 2

Write-Host "Launching Web  -> http://localhost:5173" -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$WebDir'; Write-Host 'TopZinto Web (npm run dev)' -ForegroundColor Green; npm run dev"
)

Write-Host ""
Write-Host "Waiting for API health check..." -ForegroundColor Gray

$healthy = $false
for ($i = 1; $i -le 30; $i++) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/health" -TimeoutSec 2 -ErrorAction Stop
        if ($response.status -eq "healthy") {
            $healthy = $true
            break
        }
    } catch {
        Start-Sleep -Seconds 2
    }
}

Write-Host ""
if ($healthy) {
    Write-Host "API is healthy (version $($response.version))" -ForegroundColor Green
} else {
    Write-Host "API not ready yet — check the API terminal window." -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "Open the app:  http://localhost:5173" -ForegroundColor White
Write-Host "Swagger (dev): http://localhost:5000/swagger" -ForegroundColor White
Write-Host "Login:         admin@topzinto.com / Topzinto@2024" -ForegroundColor White
Write-Host ""
Write-Host "Tip: Use 'Remember me' for a 7-day session, or leave unchecked for an 8-hour browser session." -ForegroundColor Gray
Write-Host ""
