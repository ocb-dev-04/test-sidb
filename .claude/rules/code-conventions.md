# Rule: Code conventions (C# / .NET 8 / EF Core)

Apply to all new and modified code. Where a rule below came from a previous project and conflicts with
the technical test's mandatory naming rules, **the test's rules win** (see "Naming" first).

---

## 1. Naming (mandatory — from the technical test)

| Element | Style | Example |
|---|---|---|
| Classes, records, structs, enums, enum members | PascalCase | `OrderService`, `OrderStatus.Pending` |
| Methods, properties, events | PascalCase | `CreateAsync`, `TotalAmount` |
| Interfaces | `I` + PascalCase | `IOrderRepository` |
| Locals, parameters, lambda parameters | camelCase | `orderId`, `cancellationToken`, `order => order.IsActive` |
| Constants (`const`, and `static readonly` values used as constants) | UPPER_SNAKE_CASE | `MAXIMUM_PAGE_SIZE`, `DEFAULT_CURRENCY_CODE` |
| Private instance fields | `_camelCase` | `_orderRepository` |
| Async methods | `Async` suffix | `GetByIdAsync` |
| `IAsyncEnumerable<T>` methods | `Stream` suffix | `ActiveOrdersStream` |

- **No abbreviations anywhere**: `cancellationToken` not `ct`, `options` not `opts`, `repository` not `repo`,
  `configuration` not `config`, `order => ...` not `o => ...`, `exception` not `ex`/`e`. Lambda parameters
  count — single-letter lambdas are abbreviations.
- Widely established acronyms are treated as words, PascalCase: `Id`, `Api`, `Http`, `Jwt`, `Url`, `Sql`, `Utc`
  (`JwtSettings`, `CreatedOnUtc`). Prefer a full word when one exists (`Response`/`Model` over `Dto` when in doubt).
- **No magic numbers or magic strings**: every literal with meaning becomes a named UPPER_CASE constant
  (page sizes, lengths, timeouts, retry counts, roles, claim names). Literals `0`, `1`, `-1`, `true`, `false`
  and `string.Empty` in their obvious sense are fine. Column lengths in EF configurations are constants too.
- Configuration values (connection strings, JWT key/issuer/audience, expirations) live in `appsettings.json`
  and are bound via `IOptions<T>` — never hard-coded.
- Domain property names are domain terms, never vendor names (`ExternalPaymentId`, not `StripePaymentId`).

**Previous-project patterns that must change here:** constants declared PascalCase (`public const string Value`)
→ UPPER_SNAKE_CASE; `ct`/`opts`/single-letter lambdas → full words; inline numbers such as
`TimeSpan.FromMilliseconds(500)`, `MaxRetryAttempts = 2`, `* 100m` → named constants.

---

## 2. Language and general style

- **Language split (owner decision, 2026-10-10):**
  - **Spanish only at the edges the test names**: layer/project names (`SB.Solicitudes.Dominio`,
    `.Aplicacion`, `.Servicios`, `.Infraestructura`), suggested routes (`/api/solicitudes`,
    `/api/catalogos/areas`), and storage names pointed out explicitly by mapping — EF `ToTable("Solicitudes")`
    / `HasColumnName("FechaCompromiso")` (same for the SQLite context). Excel/text-file headers
    ("Nombre", "Categoría", "Poder del Estado", "Sector") are UPPER_CASE constants in the reader, not identifiers.
  - **English** for C# types: `ServiceRequest` (Solicitud), `StatusHistory` (HistorialEstado), `Area`,
    `RequestType` (TipoSolicitud), `Comment`, `Notification`, `User`, `GovernmentEntity`; enums, properties,
    log templates and error/exception messages too.
  - **Spanish** for every code comment (`//`, `/* */`, XML docs `///`, TODOs), commit messages, README and `docs/`.
- **Comments: minimal (owner decision).** The code is simple; names must explain it. No comments that narrate
  what the code does, no section banners, no `///` on internal types, private members or obvious members.
  A comment is allowed only for a non-obvious *why* (a business rule from the test, a deliberate trade-off),
  in one line. Exception: a short one-line `/// <summary>` on public controller actions and request/response
  types, because Swagger shows it. Design rationale goes to README / `docs/arquitectura.md`, not the code.
- Nullable reference types enabled (`<Nullable>enable</Nullable>`); fix warnings in files you touch.
- Explicit types over `var` (`Order order = ...`); target-typed `new()` is fine when the type is on the left.
- Classes are `sealed` by default; `internal` unless another project needs the type. Records for immutable
  data (requests, responses, commands, DTO projections).
- Short type names + `using`; never inline-qualify a namespace (`My.App.Domain.Order`) except for a real
  name collision (then prefer a `using` alias).
- Positional arguments; named arguments only to skip optional parameters (name only the targeted one).
- Assign a request/command to a named local before passing it to a dispatcher/service call — no inline `new`
  inside the call.
- Domain entities: prefix instance members with `this.` (`this.Name = name;`). Elsewhere never `this._field`.
- Never use explicit casts to feed a factory/constructor; compute in the right type first, then pass.
- Do not leak defaults into persistence: no `.HasDefaultValue(...)` in `IEntityTypeConfiguration`; the default
  lives in the entity (initializer or factory method).
- Collections: `.Any()` / `!collection.Any()` for emptiness checks (owner preference).
- String comparison when neither side is a value object: `==`/`!=` or `string.Equals(a, b, StringComparison.Ordinal)`,
  never `a.Equals(b)` on a possibly-null instance.
- Dates: `DateTimeOffset` / UTC. Never hand `DateTimeOffset.DateTime` (Kind=Unspecified) to an API that interprets
  `Kind`; use `.UtcDateTime` (see errors.md, JWT expiry).

---

## 3. Formatting

- Max line length 120. A declaration that exceeds it wraps the assignment onto the next line, indented once:
  ```csharp
  Result<OrderResponse> order
      = await _orderService.GetByIdAsync(orderId, cancellationToken);
  ```
- **One dot per line** for method chains (LINQ, fluent builders, EF configs). A single member access may stay inline.
  ```csharp
  OrderSummary[] summaries = await _context.Orders
      .AsNoTracking()
      .Where(order => order.CustomerId == customerId)
      .OrderBy(order => order.CreatedOnUtc)
      .Select(order => new OrderSummary(order.Id, order.Total))
      .ToArrayAsync(cancellationToken);
  ```
- **One argument per line** when a call has several arguments; a single short argument may stay inline.
- **Guard clause attached to its variable** — no blank line between them; one blank line after the block.
  ```csharp
  Order? order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
  if (order is null)
      return Result.Failure<OrderResponse>(OrderErrors.NOT_FOUND);

  order.Confirm();
  ```

---

## 4. Async and cancellation

- Every I/O method is `async`, returns `Task`/`Task<T>`, ends in `Async`, and accepts a `CancellationToken`.
- `CancellationToken cancellationToken = default` is **always the last parameter** (interfaces and implementations)
  and the last argument at call sites — optional parameters go before it.
- Propagate the token to every awaited call (EF, HttpClient, services). Never `.Result` / `.Wait()`.
- `Task.WhenAll` only for independent awaits on a client-awaited path (launch → `WhenAll` → `await` each task →
  guard). Keep data-dependent awaits serial. **EF warning:** one `DbContext` instance is not thread-safe — two
  concurrent queries on the same scoped context throw "A second operation was started on this context". Only
  parallelize work that uses separate contexts (`IDbContextFactory<T>`) or non-EF dependencies.
- Background/hosted work that nobody awaits stays sequential (no latency win, only concurrency risk).

---

## 5. Errors and results

- Expected domain failures (not found, validation, conflict, unauthorized) return a `Result`/`Result<T>` with a
  typed error — no exceptions for control flow. Exceptions are for truly exceptional cases; a global exception
  handler maps them to ProblemDetails and logs via Serilog.
- Error codes/messages are constants in a per-entity `*Errors` static class (UPPER_SNAKE_CASE fields).
- Structured logging templates (`_logger.LogInformation("Order {OrderId} confirmed", orderId)`), never interpolation.

---

## 6. Nullability and defaults

- Avoid `T?` on domain state when a meaningful non-null default exists (`string.Empty`, `Guid.Empty`, `0`, `false`).
  Keep `T?` where absence is a real state (e.g. `FirstOrDefaultAsync` result checked on the next line).
- Under `[ApiController]`, a non-nullable `string` request member **without a default** is implicitly `[Required]`
  (400 when omitted). Optional request members need an explicit default (`string Search = ""`) and go last.
- A date that can be absent is never compared raw (`ValidUntil < now` is true for `default`); put the range rule in
  the entity, including the "is set" guard.

---

## 7. EF Core — queries

- Read queries: `AsNoTracking()`; project with `.Select(...)` **in SQL**, never load full entities to project in memory.
- Materialize with `ToArrayAsync` (results are read-only; return `IReadOnlyList<T>`), not `ToListAsync`.
- List endpoints project a flat shape (scalars only); by-id endpoints project the full detail. Never `Include(...)`
  a navigation the projection does not use.
- `FirstOrDefaultAsync(predicate)` takes **at most one** comparison; compound predicates go into a preceding
  `.Where(...)` and `FirstOrDefaultAsync(cancellationToken)` takes no predicate.
- `FindAsync` with a token: `FindAsync([id], cancellationToken)` — `FindAsync(id, cancellationToken)` packs the token
  as a second key value (see errors.md).
- `OrderBy` on entity columns **before** `Select` (ordering on a projected DTO can be untranslatable).
- In nested projections inline the inner constructor (`order.Lines.Select(line => new LineResponse(...))`); do not
  embed a reusable `Expression` via `.AsQueryable().Select(...)`.
- Never `ToDictionary` on a key that the database does not enforce as unique — use `ToLookup` or verify the index.
- Global query filters must reference a **DbContext instance property**, never a static/ambient member (a static is
  baked into the cached query plan as a constant — the first caller's value becomes everyone's).
- Every lookup by id answers "whose data can this return?" — scope reads to the authenticated user unless the role
  legitimately crosses owners (admin).

### Pagination

- Every collection endpoint is paginated; no unbounded queries.
- Parameters: `pageNumber` / `pageSize` (or a keyset `cursor`), validated; `pageSize` capped by a constant
  (`MAXIMUM_PAGE_SIZE`) with a default constant (`DEFAULT_PAGE_SIZE`).
- Deterministic order is mandatory before `Skip`/`Take` (order by a unique column last, e.g. `Id`).
- Response carries items + paging metadata (`TotalCount`/`HasNextPage`), as a record.
- Prefer keyset (`WHERE Id > @lastId`) for large/append-heavy tables; offset is fine for small admin lists.

---

## 8. EF Core — writes and model

- Write methods operate on one aggregate per call: load → mutate via entity method → `SaveChangesAsync`.
- `ExecuteUpdateAsync`/`ExecuteDeleteAsync` skip change tracking (no domain events, no interceptors, no audit
  fields set by `SaveChanges`) and do not run on the InMemory provider — use only when none of that matters.
- Configurations in `IEntityTypeConfiguration<T>` classes, one per entity; lengths/precision as constants.
- A domain type must not share its simple name with a type from another namespace the entities import (generated
  code/compiled models will hit CS0104).
- Migrations are generated/applied only when the owner asks.

---

## 9. Dependency injection and configuration

- Constructor injection; register by interface. Each layer exposes one `Add{Layer}Services(this IServiceCollection, IConfiguration)`.
- Options classes bound from `appsettings.json` with `ValidateOnStart()`; a `SECTION_NAME` constant per options class.
- Do not rely on `AppDomain.CurrentDomain.GetAssemblies()` for registration scans (load-order dependent); scan
  `typeof(Marker).Assembly` explicitly.
- Response compression (if enabled): Brotli first, then Gzip. External HTTP clients get a resilience handler
  (timeout + retry; values as constants).
