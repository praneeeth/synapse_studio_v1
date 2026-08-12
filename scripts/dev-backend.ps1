# scripts/dev-backend.ps1
# Creates the venv on first run, then starts the API with auto-reload.
$ErrorActionPreference = "Stop"
$backend = Join-Path $PSScriptRoot "..\backend"
Set-Location $backend

if (-not (Test-Path ".\.venv\Scripts\python.exe")) {
    Write-Host "Creating virtualenv..."
    python -m venv .venv
    .\.venv\Scripts\python.exe -m pip install --upgrade pip
    .\.venv\Scripts\python.exe -m pip install --index-url https://download.pytorch.org/whl/cpu torch
    .\.venv\Scripts\python.exe -m pip install -r requirements-dev.txt
}

if (-not (Test-Path ".env")) {
    Copy-Item ".env.example" ".env"
    Write-Host "Created backend/.env from .env.example -- add your GROQ_API_KEY before chatting."
}

.\.venv\Scripts\python.exe -m uvicorn app.main:app --reload --port 8000
