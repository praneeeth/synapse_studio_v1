# scripts/test-backend.ps1
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..\backend")
.\.venv\Scripts\python.exe -m pytest @args
