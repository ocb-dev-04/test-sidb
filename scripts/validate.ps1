# Validación local equivalente al pipeline de CI: compila y prueba backend y frontend.
# Uso: pwsh ./scripts/validate.ps1 [-SkipIntegration]
param(
    [switch]$SkipIntegration
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-Step([string]$title, [scriptblock]$action) {
    Write-Host "==> $title" -ForegroundColor Cyan
    & $action
    if ($LASTEXITCODE -ne 0) { throw "Falló: $title" }
}

Push-Location (Join-Path $root 'src/backend')
try {
    Invoke-Step 'Backend: build' { dotnet build SB.Solicitudes.sln -c Release }
    Invoke-Step 'Backend: pruebas unitarias y de arquitectura' { dotnet test SB.Solicitudes.sln -c Release --no-build --filter 'Category!=Integration' }
    if (-not $SkipIntegration) {
        Invoke-Step 'Backend: pruebas de integración (requiere Docker)' { dotnet test tests/SB.Solicitudes.Servicios.IntegrationTests -c Release --no-build }
    }
}
finally { Pop-Location }

Push-Location (Join-Path $root 'src/frontend')
try {
    Invoke-Step 'Frontend: dependencias' { npm ci }
    Invoke-Step 'Frontend: lint' { npm run lint }
    Invoke-Step 'Frontend: tipos' { npm run typecheck }
    Invoke-Step 'Frontend: pruebas' { npm test }
    Invoke-Step 'Frontend: build' { npm run build }
}
finally { Pop-Location }

Write-Host 'Validación completa.' -ForegroundColor Green
