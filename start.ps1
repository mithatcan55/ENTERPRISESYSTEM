# ─────────────────────────────────────────────────────────────
#  EnterpriseSystem — Tek komutla başlat
#  Kullanım: .\start.ps1
# ─────────────────────────────────────────────────────────────

$root = $PSScriptRoot

Write-Host ""
Write-Host "  EnterpriseSystem baslatiliyor..." -ForegroundColor Cyan
Write-Host ""

# Varsa çalışan process'leri temizle
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

# ── Backend (arka planda) ─────────────────────────────────────
Write-Host "  [1/2] Backend baslatiliyor (http://localhost:5279)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList `
    "-NoExit", `
    "-Command", "Set-Location '$root'; dotnet run --project src/Host.Api/Host.Api.csproj --launch-profile http" `
    -WindowStyle Normal

# Backend hazır olana kadar bekle (max 30 sn)
$backendReady = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:5279/health" -UseBasicParsing -TimeoutSec 1 -ErrorAction Stop
        if ($r.StatusCode -eq 200) { $backendReady = $true; break }
    } catch { }
    Write-Host "  Backend bekleniyor... ($($i+1)s)" -ForegroundColor DarkGray
}

if (-not $backendReady) {
    Write-Host "  [UYARI] Backend 30 saniyede hazir olmadi, frontend yine de baslatiliyor." -ForegroundColor Red
} else {
    Write-Host "  [OK] Backend hazir." -ForegroundColor Green
}

# ── Frontend (arka planda) ────────────────────────────────────
Write-Host "  [2/2] Frontend baslatiliyor (http://localhost:4173)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList `
    "-NoExit", `
    "-Command", "Set-Location '$root\frontend'; npm run dev" `
    -WindowStyle Normal

Start-Sleep -Seconds 3

Write-Host ""
Write-Host "  ✓ Hazir!" -ForegroundColor Green
Write-Host "  Frontend  → http://localhost:4173" -ForegroundColor Cyan
Write-Host "  Backend   → http://localhost:5279" -ForegroundColor Cyan
Write-Host "  Kullanici → core.admin / 123456" -ForegroundColor Cyan
Write-Host ""
