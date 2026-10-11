# CLAUDE.md — SB.Solicitudes (Superintendencia de Bancos technical test)

Senior full-stack technical test. Conversation with the owner is in **Spanish**.

## Source of truth

`Dev-Sr/` (git-ignored, confidential) holds the requirements. Re-read before deciding scope:

- `Prueba Tecnica-2.pdf` — functional/non-functional requirements, data model, suggested routes, evaluation weights.
- `API - ESPECIFICACIONES TECNICAS.pdf` — **mandatory** norms: Onion layers, project naming, naming rules.
- `Instrucciones.txt` — government-entities CRUD (from `recursos/ListaEntidadesGubernamentales.xlsx`), UI per
  `Maqueta.jpeg` (blue `rgba(13, 48, 72, .9)`, gray `rgba(237, 240, 247)`, `recursos/home.svg`, SB logo),
  conceptual answers go by **email**.

## Hard constraints (never violate)

- .NET 8 (`net8.0`), ASP.NET Core Web API, EF Core, SQL Server for Solicitudes; JWT bearer with roles
  Administrador / Analista / Solicitante; Serilog; Swagger with Bearer; basic exception handling.
- Projects are `SB.Solicitudes.{Dominio,Aplicacion,Infraestructura,Servicios}` (Servicios = Web API host).
  Onion dependencies point inward: Dominio ← Aplicacion ← Infraestructura ← Servicios. Enforced by
  `tests/SB.Solicitudes.ArchitectureTests`.
- Government entities: plain text file inside the project is the default store; SQLite and Excel are extra
  sources behind a strategy + keyed-services factory (`GovernmentEntityDataSource`). Excel is read-only.
- Naming (enforced as build errors by `.editorconfig`): PascalCase types/members, camelCase locals/params,
  `_camelCase` private fields, **UPPER_SNAKE_CASE constants and static readonly fields**, `I` prefix,
  **no abbreviations**, **no magic numbers**, connection strings in `appsettings.json`.
- No dependency on the owner's private NuGets (OCB.*, Shared.Common.Helper, Value.Objects.Helper);
  `src/backend/nuget.config` pins nuget.org only. Port ideas as small in-repo code.

## Language

English C# types; Spanish only at named edges (layers, routes, EF `ToTable`/`HasColumnName`, file headers);
Spanish for comments, commits, README and `docs/`. Comments minimal. Details: `rules/code-conventions.md` §2.

## Layout

```
src/backend/      SB.Solicitudes.sln, global.json, Directory.*.props, nuget.config
  src/            the four layer projects
  tests/          ArchitectureTests (+ unit / integration test projects as they get tests)
src/frontend/     React + TypeScript (Vite)
database/         schema.sql (sample data is loaded at startup by DatabaseSeeder, only into empty tables)
docs/             arquitectura.md, evidencias.md
```

## Commands (from `src/backend`)

- Build: `dotnet build SB.Solicitudes.sln` (warnings are errors)
- Test: `dotnet test SB.Solicitudes.sln` (integration tests need Docker; skip with `--filter "Category!=Integration"`)
- SDK: `global.json` targets 8.0.100 with `rollForward: latestMajor` (builds on SDK 8 or newer).
- EF: `dotnet tool restore`, then `dotnet ef migrations add <Name> --context ApplicationDbContext --project src/SB.Solicitudes.Infraestructura --startup-project src/SB.Solicitudes.Infraestructura --output-dir Persistence/Migrations`.
  SQLite store: same command with `--context GovernmentEntitiesDbContext --output-dir GovernmentEntities/Migrations`.
  Regenerate `database/schema.sql` with `dotnet ef migrations script 0 --idempotent ...` after any migration.
- Whole stack: `docker compose up --build` at the repo root (SQL Server 1433, API 5080, frontend 8080).
- Full local check: `scripts/validate.ps1` (or `.sh`).
- Demo users: `admin@`, `analista1@`, `analista2@`, `solicitante1@`, `solicitante2@solicitudes.local`, password `Demo2026*`.

## Rules to read before working

- `rules/code-conventions.md`, `rules/test-conventions.md`, `rules/edit-and-commit-workflow.md`
- `error-to-avoid.md` (agent mistakes), `errors.md` (runtime/build errors with fixes)

## Workflow

Phased plan agreed with the owner: 0 base → 1 Dominio → 2 Aplicacion → 3 Infraestructura → 4 Servicios →
5 tests → 6 frontend → 7 delivery (docker, CI, scripts, README, ADRs) → 8 conceptual-answers email draft.
Commits: Conventional Commits in Spanish, foundation one per piece, then vertical slices; never push.
