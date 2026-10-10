#!/usr/bin/env bash
# Validación local equivalente al pipeline de CI: compila y prueba backend y frontend.
# Uso: ./scripts/validate.sh [--skip-integration]
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
skip_integration="${1:-}"

echo "==> Backend: build"
(cd "$root/src/backend" && dotnet build SB.Solicitudes.sln -c Release)

echo "==> Backend: pruebas unitarias y de arquitectura"
(cd "$root/src/backend" && dotnet test SB.Solicitudes.sln -c Release --no-build --filter "Category!=Integration")

if [ "$skip_integration" != "--skip-integration" ]; then
  echo "==> Backend: pruebas de integración (requiere Docker)"
  (cd "$root/src/backend" && dotnet test tests/SB.Solicitudes.Servicios.IntegrationTests -c Release --no-build)
fi

echo "==> Frontend"
(cd "$root/src/frontend" && npm ci && npm run lint && npm run typecheck && npm test && npm run build)

echo "Validación completa."
