# scripts/dev-frontend.ps1
$ErrorActionPreference = "Stop"
$frontend = Join-Path $PSScriptRoot "..\frontend"
Set-Location $frontend

if (-not (Test-Path "node_modules")) {
    npm install
}
if (-not (Test-Path ".env") -and -not (Test-Path ".env.local")) {
    Copy-Item ".env.example" ".env"
}

npm run dev
