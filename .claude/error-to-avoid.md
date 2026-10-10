# Errors to avoid — agent working mistakes

Companion to `.claude/errors.md`. That file records **runtime/build errors of the codebase** (root cause + fix).
This one records **mistakes the agent makes while working** — wrong assumptions, tool misuse, rule violations —
distilled into rules. Seeded from a previous project's lessons; **(R)** = happened more than once there.

Read this before implementing, refactoring, writing tests or editing docs. Add an entry whenever a mistake is
caught — same shape: rule first, then *why* in one line.

---

## 1. Assumptions — read, never guess

- **(R) Never invent a member, type or API.** Read the class/record (or Grep `class X`/`record X`) before using it.
  *Why:* guessed names (non-existent properties, extension methods, error classes) cost a full build cycle each.
- **Never pick a NuGet package version by assumption** — check what is actually available and compatible with
  net8.0 (`dotnet list package --outdated`, an actual restore). *Why:* a lagging search index ≠ unpublished; a
  newer major may target a newer framework or change license.
- **Check paths and project names before using them** — Glob before running sed/grep on a guessed path.
  *Why:* commands on non-existent paths "succeed" silently.
- **Find the file the runtime actually loads before editing a "source of truth".** *Why:* a doc copy was edited
  for months while the app loaded another folder.
- **Verify a data source's shape before designing on it** (API payload, seed file, appsettings section).
- **Verify a reported "gap" against the code before reporting it** — docs go stale.

## 2. Edit tool and file rules

- **(R) One Edit per file per response** (`rules/edit-and-commit-workflow.md`). *Why:* the second Edit can
  silently not reach disk.
- **(R) Don't Write over a pre-existing file** — targeted Edits; ask before a real rewrite.
- **(R) No inline namespace-qualified types to dodge a `using`.** Grep touched files for fully-qualified
  project namespaces before committing.
- **(R) Regex bulk replaces over-match.** Dry-run first (print matches per file), scope to an explicit file list,
  assert the expected count per anchor, review the diff before building.
- **Don't parse C# with bracket counters in scripts** (`=>` breaks `<>` depth) — let compiler errors drive
  targeted Edits.
- **Never edit by line number** — lines shift after any earlier edit.
- **Never bulk-tick a checklist** (`- [ ]` → `- [x]` everywhere) — it ticks owner-run items. One at a time.
- **Copy anchors from a fresh Grep, not from memory.**

## 3. Signature changes, usings, builds

- **(R) Before changing a public signature (record, constructor, interface, DTO) grep ALL consumers first**:
  `new TypeName(`, target-typed `new(` assigned to that type, `with { ... }` copies, and every test project.
  Then build the whole solution, not one project. *Why:* test projects were repeatedly forgotten.
- **(R) Moving/adding a type or extension → add every `using` in the same step.** Keep extension methods in the
  namespace of the type they extend.
- **`dotnet build` takes one project/solution** — build the `.sln`.
- **Chain dependent steps with `&&`, not `;`** (Bash) / `; if ($?) { ... }` (PowerShell 5.1). *Why:* a failed
  script followed by `; dotnet build` printed "Build succeeded" over the traceback. Any Traceback/`error` = failed.
- **Fix nullable warnings in the files you touch** before committing.
- **Moving a project on disk ≠ moving it in the solution.** The `.sln` stores the path AND the solution folder
  (`NestedProjects`); verify both after a move (`dotnet sln list`).
- **Wrapping a return value in `Result<T>`/`Optional<T>` does not break generic sinks** (`Serialize<T>`,
  `Returns(x)`, `Ok(x)`) — the build stays green while the wrapper itself gets serialized. Grep consumers and
  unwrap explicitly.
- **A target-typed `condition ? x : null` into a wrapper class compiles and yields a real `null`.** Write both
  arms explicitly.

## 4. Tests

- **When a domain rule changes, update shared test builders in the same step** and run every consumer test project.
- **Adding a validation/invariant → grep the tests that build that entity/request** and give them valid data.
- **Don't read a substitute inside another substitute's `.Returns(...)`** — compute into a local first.
- **EF InMemory hides SQL problems** (translation, constraints, `ExecuteUpdate`) — use a relational test database
  for query tests.

## 5. Shell on Windows (Git Bash / PowerShell 5.1)

- **(R) No inline heredocs for Python or mixed quoting** — write the script to the scratchpad and run the file.
  *Why:* heredocs mangled backslashes, quotes and non-ASCII characters.
- **(R) `PYTHONIOENCODING=utf-8`** on every python call; open files as UTF-8; preserve BOM and CRLF on write.
- **(R) No Windows `\` paths inside sed/awk regexes or replacements** — use `/` (MSBuild accepts forward slashes).
- **Never `$`-anchor sed/perl patterns on CRLF files** — they silently no-op. Verify every scripted edit by
  grepping the NEW text afterwards; exit code 0 proves nothing.
- **(R) Never bundle `rm -rf` into a command that does other work** — the whole command gets denied. Ask the
  owner to delete caches; avoid throwaway artefacts that need it.
- **PowerShell 5.1 has no `&&`/`||`/`?:`/`??`**; `Set-Content` defaults to ANSI — pass `-Encoding utf8`.
- **`.ps1` files: ASCII only** (no em-dash/smart quotes) or UTF-8 with BOM — see errors.md.
- `tar` reads `C:` as a remote host → `--force-local`. Check tools with `command -v` (`jq` may be missing; fall back
  to Python `json`). `appsettings.json` allows comments — a strict `json.load` failing is not your edit's fault.

## 6. Design — ask before building

- **Any lookup by id: "whose data can this return?"** Scope to the JWT user by default; never trust client-sent
  ownership data.
- **Trace one real request end to end** (input → handler → DB → response) and look for fail-open `catch` blocks.
- **Respect the existing model before proposing new identity/schema schemes** — avoid over-engineering.
- **Question suspicious endpoints before extending them** (no ownership check, no consumer).
- **Present options before building infrastructure** when there is a real trade-off.

## 7. Communication and scope

- **(R) Conversational replies in Spanish.** Code comments and commit messages in Spanish too; identifiers in
  English except what the test names in Spanish (see `rules/code-conventions.md` §2). Re-assert after context compaction.
- **(R) Define every new term in one line with a concrete example.** One short question per bullet.
- **Restate the exact scope of a multi-part instruction in one line before acting.**

## 8. Subagents

- **Name read-only inputs and writable outputs explicitly in the prompt.** *Why:* a subagent overwrote a file it
  was only meant to read.
- **Always `git diff --stat` after an agent writes**, and verify its claims (build/test counts) yourself.
- Tell parallel agents which files each one owns (avoids concurrent edits and `dotnet build` file locks).
