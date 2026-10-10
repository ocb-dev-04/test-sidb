# Rule: Edit and commit workflow

## Plan before non-trivial changes

- Task spanning more than 2 files, a new feature, or a refactor: show a short checklist (and options with
  pros/cons when there is more than one valid approach) and wait for explicit approval before touching files.
- "Explain X" / "what do you think about X": explain root cause and impact, propose options, wait for approval.
- Quick single-file fixes and pure questions skip this.

## Edit tool

1. **Never issue more than one Edit per file per response.** A second Edit on the same file in the same
   response can report success without reaching disk. Edits to different files in one response are safe.
2. Several changes in one file: one Edit with `replace_all` for a repeated identical pattern; otherwise serialize
   (Edit → build or Grep → Edit).
3. **Never rewrite (Write) a file you did not just create.** Use targeted Edits; ask before a real rewrite.
4. After editing a file other code depends on, verify with a targeted Grep before the next dependent step.
5. If a build reveals a "lost" edit: Read the file, re-apply serially, rebuild. Never assume it applied.
6. Copy `old_string` anchors from a fresh Read/Grep, not from memory (trailing spaces, duplicate matches).
7. Never edit by line number (`sed -i '89d'`) — lines shift after any earlier edit.

## Implementation sequence (never reorder)

1. Implement.
2. Review the tests that cover the changed code; add/update tests for new paths.
3. Build the **whole solution** (`dotnet build` at the `.sln`) — 0 errors, no new warnings in touched files.
4. Test — `dotnet test` on the affected projects; all green. For a public signature change run every test project.
5. Commit (below). Build/test may be skipped only for pure docs/non-code changes.

## Commits

- **Conventional Commits, message in Spanish**: `type(scope): resumen en imperativo` — lower case, no trailing
  period, ≤ 72 chars. Types stay in English: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `build`, `ci`,
  `perf`. Scope = layer or feature (`feat(solicitudes): agrega listado paginado con filtros`,
  `test(servicios): cubre fallos de login JWT`).
- Body (optional, Spanish): what changed and why, wrapped at 72.
- Grouping: foundation commits one per piece, then features as vertical slices (Dominio → Aplicacion →
  Infraestructura → Servicios + tests), each leaving the build green.
- Stage **only** the files of this change, by explicit path — never `git add -A` / `git add .` blindly.
- Trailer: `Co-Authored-By: Claude <model-name> <noreply@anthropic.com>`, where `<model-name>` is the model
  actually running (read the "You are powered by the model named ..." context line; a subagent's commit names
  the subagent's model).
- Commit only when the owner asks or the agreed workflow says so. **Never push**, never force anything,
  never amend a commit you did not just create, never skip hooks.
- Never use `git checkout -- <file>` / `git restore` to "recover" work: uncommitted new files are not in git.
- EF migrations are not part of this workflow — generate/apply them only when the owner explicitly asks.

## Definition of done

- Proven working: build + tests green, and the behavior demonstrated (test output, request/response).
- "Would a staff engineer approve this?" — minimal impact, root cause fixed, no temporary hacks.
