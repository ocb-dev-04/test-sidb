# Known errors and fixes

Runtime/build errors with root cause and fix. Before debugging a failing build/test or odd behavior, scan
this file. After fixing a new error, append it using the template at the bottom.

Entries G001–G016 are seeded from a previous .NET/EF Core project: they are generic and **have not occurred
in this repository yet** (type names in examples are illustrative). Number new local entries E001, E002, ...

| Code | Symptom (one line) | Category |
|---|---|---|
| G001 | `FindAsync(id, cancellationToken)` returns null for a saved entity | EF Core |
| G002 | `Substitute.For<ILogger<T>>()` throws `ArgumentException` (cannot create proxy) | tests |
| G003 | `ExecuteUpdateAsync`/`ExecuteDeleteAsync` throw under EF InMemory | EF Core / tests |
| G004 | SQLite `EnsureCreated` skips tables for every DbContext after the first | EF Core / tests |
| G005 | Nested `IQueryable<TDto>` projection rejected ("must be IEnumerable") | EF Core |
| G006 | `OrderBy` after `Select` into a DTO with a subquery is untranslatable | EF Core |
| G007 | `ToDictionary` throws duplicate-key `ArgumentException` | LINQ |
| G008 | JWT lives hours longer than configured on a dev machine | auth |
| G009 | `NETSDK1022` duplicate `Content` items in a Web SDK project | build |
| G010 | `ReflectionTypeLoadException` at startup from a stale DLL in `bin/` | build |
| G011 | Text files corrupted to `?` / mojibake after a scripted write | encoding |
| G012 | `JsonReaderException '0xEF' is an invalid start of a value` | encoding |
| G013 | `.ps1` fails to parse: "string is missing the terminator" on a correct line | tooling |
| G014 | `sed` edit corrupts paths or silently changes nothing | tooling |
| G015 | Adding a parameter breaks hundreds of mock stubs (CS1503) | tests |
| G016 | Generated code fails with CS0104 ambiguous reference | build |

---

## G001 — `FindAsync(id, cancellationToken)` returns null
**Root cause:** resolves to `FindAsync(params object?[] keyValues)` and packs the token as a second key value.
**Fix:** `await context.Orders.FindAsync([id], cancellationToken);` (array → `FindAsync(object?[], CancellationToken)`).

## G002 — `Substitute.For<ILogger<T>>()` throws `ArgumentException` "Can not create proxy ... is not accessible"
**Root cause:** `T` is `internal`; Castle.DynamicProxy cannot proxy it and `Microsoft.Extensions.Logging.Abstractions`
is strong-named, so `InternalsVisibleTo("DynamicProxyGenAssembly2")` would need the full public key.
**Fix:** `NullLogger<T>.Instance`. Logging is never asserted.

## G003 — `The methods 'ExecuteUpdate' and 'ExecuteUpdateAsync' are not supported by the current database provider`
**Root cause:** EF InMemory does not implement set-based operators; it throws at translation time on every call.
**Fix:** cover those methods with a relational test database (SQL Server Testcontainers/LocalDB). Do not revert
the set-based operator just to keep an InMemory test green; confirm any removed coverage with the owner.

## G004 — SQLite `no such table: X` with several DbContexts on one connection
**Root cause:** `EnsureCreatedAsync` is a no-op if the database already has ANY table.
**Fix:** `context.GetService<IRelationalDatabaseCreator>().CreateTablesAsync()` per context (catch "already exists").

## G005 — "The query contains a projection of type 'IQueryable<TDto>'. Collections in the final projection must be an 'IEnumerable<T>'"
**Root cause:** `parent.Children.AsQueryable().Select(ChildDto.Projection)` nested inside a parent projection.
**Fix:** inline the constructor: `parent.Children.Select(child => new ChildResponse(child.Id, child.Name))`.

## G006 — "The LINQ expression 'OrderBy(...new Dto(...))' could not be translated"
**Root cause:** `OrderBy` after `Select` forces the projected DTO (with a correlated subquery) into `ORDER BY`.
**Fix:** order on the entity column before projecting: `.OrderBy(country => country.Name).Select(...)`.

## G007 — `ArgumentException: An item with the same key has already been added`
**Root cause:** `ToDictionary` on a key the data does not guarantee unique (labels, codes reused across parents).
**Fix:** `ToLookup` and iterate all matches, or confirm a unique index first.

## G008 — JWT configured for 5 minutes stays valid for hours locally
**Root cause:** `expires: expiresOn.DateTime` — `DateTimeOffset.DateTime` has `Kind=Unspecified`; `JwtSecurityToken`
calls `ToUniversalTime()`, treating it as local time and shifting `exp` by the machine's UTC offset.
**Fix:** pass `expiresOn.UtcDateTime`. Diagnose "token never expires" by decoding `exp` first, before auditing validation.

## G009 — `NETSDK1022: Duplicate 'Content' items were included`
**Root cause:** the Web SDK already includes project-folder files as implicit `Content`.
**Fix:** `<Content Update="file.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>`
(`Include` only for files outside the project folder).

## G010 — `ReflectionTypeLoadException` ("Unable to load one or more of the requested types") at `builder.Build()`
**Root cause:** a renamed/removed project or retargeted TFM leaves its old DLL in `bin/`; incremental builds never
delete it, and assembly scanning (`GetAssemblies()`/`GetTypes()`) loads it.
**Fix:** delete `bin/` and `obj/` of the affected projects (ask the owner), rebuild. Read `LoaderExceptions` for detail.

## G011 — Non-Latin text replaced by `?` (or mojibake) after a script/tool wrote the file
**Root cause:** written with ANSI/Latin-1 (PowerShell 5.1 `Set-Content` default, Python without `encoding=`). Lossy.
**Fix:** recover from git history; always write UTF-8 (`-Encoding utf8`, `open(path, "w", encoding="utf-8")`).
Validate by scanning values for `?` runs, not just key counts.

## G012 — `JsonReaderException: '0xEF' is an invalid start of a value. LineNumber: 0`
**Root cause:** JSON file saved with a UTF-8 BOM read via bytes (`File.ReadAllBytes` → `Utf8JsonReader`).
**Fix:** save JSON as UTF-8 without BOM (check `head -c 3 file | od -An -tx1` is not `ef bb bf`), or skip the BOM.

## G013 — PowerShell 5.1: "The string is missing the terminator" pointing at a correct line
**Root cause:** BOM-less `.ps1` is read as Windows-1252; an em-dash's byte `0x94` decodes as a smart quote that
closes a string early.
**Fix:** keep `.ps1` ASCII-only or save with UTF-8 BOM. Find the first non-ASCII char: `grep -nP '[^\x00-\x7F]' file.ps1`.

## G014 — `sed` replacement corrupts paths, or reports success but changes nothing
**Root cause:** (1) backslashes lose one escaping level through the tool call; `\c` in a sed replacement becomes a
control char. (2) `$`-anchored patterns do not match CRLF lines.
**Fix:** forward slashes in paths; no `$` anchors on CRLF files (or `\r?$` in perl); grep the new text afterwards.

## G015 — Hundreds of `CS1503 Argument N: cannot convert from 'CancellationToken' to ...` in test files
**Root cause:** a new optional parameter inserted before the last (`cancellationToken`) shifts every full-positional
mock stub. (Appending after the last one instead compiles but can silently stop stubs from matching.)
**Fix:** let compiler diagnostics drive a mechanical fix (dedupe by file+line, insert `Arg.Any<T>()` bottom-up),
rebuild, fix the residue by hand. Budget it in the same commit.

## G016 — `CS0104: 'X' is an ambiguous reference between 'A.X' and 'B.X'` in generated code
**Root cause:** two types share a simple name; a generator (EF compiled models, scaffolding) emits short names + usings.
**Fix:** rename the local type to a unique name; never hand-edit generated files.

---

## E001 — `NU1507: There are 2 package sources defined ... When using central package management, map your package sources`
**Symptom:** first restore of the solution failed (warnings are errors).
**Root cause:** the machine-wide NuGet config adds a private GitHub feed; with CPM NuGet demands source mapping.
**Fix:** `src/backend/nuget.config` with `<clear/>`, only nuget.org and a `packageSourceMapping` for `*`.
**Rule:** never depend on machine-level feeds; reviewers must restore from nuget.org only.

## E002 — Integration tests ignored `ConfigureAppConfiguration` overrides (429 on login, wrong database)
**Symptom:** tests hit the developer SQL Server and the login rate limit, although the factory overrode both.
**Root cause:** `AddInfrastructure`/`AddPresentation` read configuration eagerly while registering services, before
`WebApplicationFactory` applied its in-memory configuration.
**Fix:** resolve configuration lazily: `AddDbContext((serviceProvider, options) => ...)` reading `IConfiguration`,
`AddOptions<JwtBearerOptions>().Configure<IOptions<JwtOptions>>`, CORS via `AddOptions<CorsOptions>()`, rate limit
options resolved from `httpContext.RequestServices`.
**Rule:** never call `configuration.GetConnectionString/GetValue` at registration time; prove test isolation by dropping
the local database and checking it is not recreated.

## E003 — `CA1707` / `CA1716` / `CA1711` break the build on spec-mandated names
**Root cause:** analyzers forbid `_` in public members (UPPER_SNAKE constants), the VB keyword `Error`, and suffixes
`EventHandler`, `Delegate`, `Collection`.
**Fix:** CA1707 and CA1716 disabled in `.editorconfig` (they contradict the specification); for CA1711 the types were
renamed (`IDomainEventSubscriber`, `RequestPipelineNext`, `ApiTestGroup`).

## E004 — Generated `[LoggerMessage]` fields fail `ConstantsShouldBeUpperCase`
**Root cause:** the source generator emits `private static readonly ... __XCallback` fields in the partial class.
**Fix:** the architecture test skips fields marked with `GeneratedCodeAttribute`.

## E005 — Docker build of the API ignores naming rules / fails on CA1707
**Root cause:** `.editorconfig` lives at the repo root, outside a `src/backend` build context.
**Fix:** the API image builds with the repo root as context (`docker-compose.yml`: `context: .`,
`dockerfile: src/backend/Dockerfile`) and copies `.editorconfig`; a root `.dockerignore` trims the context.

---

## Template

```
## E00N — <one-line symptom / exact error message>
**Symptom:** <what was observed, where>
**Root cause:** <why it happens>
**Fix:** <what was changed; code snippet if load-bearing>
**Rule:** <how to prevent it next time>
**Files fixed:** <paths>
```
