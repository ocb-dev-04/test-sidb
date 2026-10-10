# Rule: Test conventions (xUnit)

Apply every time a test is written or modified. Naming rules from `code-conventions.md` apply to tests too
(no abbreviations, constants UPPER_SNAKE_CASE, camelCase locals).

---

## Projects and layout

- One unit-test project per production layer that has logic (e.g. `*.Application.Tests`, `*.Domain.Tests`) plus
  one integration-test project for the API/persistence (`*.Api.IntegrationTests`).
- Folder layout mirrors the code under test (`Services/Orders/`, `Validators/`, `Entities/`).
- Shared builders/fakes live at the project root (`TestData.cs`, `OrderBuilder.cs`). When a domain rule changes,
  update the shared builders **in the same step** and run every consumer test project.
- `[assembly: InternalsVisibleTo("X.Tests")]` in projects whose `internal sealed` types are tested.
- Packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, `NSubstitute`
  (mocks), an assertion library, `Bogus` (fake data) if useful. **FluentAssertions 8+ is commercially licensed** —
  pin 7.x or use plain `Assert`/Shouldly; decide once and stay consistent.

## Naming and structure

- Test class: `{ClassUnderTest}Tests`, `public sealed`.
- Test method: PascalCase, no underscores (the test requires PascalCase methods):
  `{Method}Should{Outcome}When{Condition}` — e.g. `CreateAsyncShouldReturnFailureWhenEmailAlreadyExists`.
  (The `Method_Should_X_When_Y` underscore idiom is common in .NET but violates the stated rule — avoid it here.)
- Arrange / Act / Assert, separated by a blank line; one behavior per test.
- One `[Fact]` per distinct execution path (success + each failure branch). `[Theory]` + `[InlineData]` for the same
  rule over several inputs (validators).
- No magic values: expected values are named locals or UPPER_SNAKE_CASE constants of the test class.

```csharp
public sealed class OrderServiceTests
{
    private const string EXISTING_EMAIL = "existing@example.com";

    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly OrderService _orderService;

    public OrderServiceTests()
        => _orderService = new OrderService(
            _orderRepository,
            NullLogger<OrderService>.Instance);

    [Fact]
    public async Task CreateAsyncShouldReturnFailureWhenEmailAlreadyExists()
    {
        CreateOrderRequest request = new(EXISTING_EMAIL);
        _orderRepository
            .ExistsByEmailAsync(EXISTING_EMAIL, Arg.Any<CancellationToken>())
            .Returns(true);

        Result<OrderResponse> result = await _orderService.CreateAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
```

## Mocking (NSubstitute)

- `Substitute.For<IInterface>()` — mock interfaces only, never concrete classes.
- Loggers: always `NullLogger<T>.Instance`. `Substitute.For<ILogger<T>>()` throws when `T` is `internal` (Castle
  proxy + strong-named assembly); logging is never asserted anyway.
- Read the record/interface before writing `Arg.Is<T>(x => x.Property == ...)` — never guess property names.
- Do not read one substitute inside another substitute's `.Returns(...)` — compute into a local first
  (`CouldNotSetReturnDueToTypeMismatchException`).
- An unconfigured async mock returns a completed task with `default(T)` — `null` for classes such as `Result<T>`.
  Any path that now reaches a dependency (e.g. after parallelizing with `Task.WhenAll`) must stub it with a real value.
- Assertions that encode call order (`DidNotReceive()` on a second call) break when calls become parallel — re-check them.
- `DidNotReceiveWithAnyArgs()` to assert "never called at all".
- Adding a parameter **before** the last one of a widely-mocked method breaks every positional stub at compile time;
  appending after the last one can silently stop stubs from matching at runtime. Budget the test fix in the same commit.

## EF Core in tests

- **EF InMemory is for simple CRUD only.** It does not translate SQL, ignores constraints/complex properties, is
  permissive with LINQ shapes that SQL Server rejects, and throws on `ExecuteUpdateAsync`/`ExecuteDeleteAsync`.
- Query-translation, constraints, transactions and pagination are tested against a real relational provider:
  SQL Server via Testcontainers (`Testcontainers.MsSql`) or LocalDB; SQLite in-memory only as a last resort
  (different SQL dialect — see errors.md for SQLite-specific failures).
- Throwaway test databases may use `EnsureCreatedAsync()` instead of migrations; never in production code.

## Integration tests (API)

- `WebApplicationFactory<Program>` (expose `public partial class Program;`) with the connection string overridden
  to the test database; authenticate with a JWT minted by the app's own token service, never a hand-rolled token.
- Mark the class `[Trait("Category", "Integration")]` so the fast loop can run `--filter "Category!=Integration"`.
- `[CollectionDefinition]` must live in the **same assembly** as the `[Collection]` that uses it (otherwise:
  "The following constructor parameters did not have matching fixture data").
- Assert through the public contract (HTTP status + body) and, when needed, a fresh `DbContext` read — not via
  the same tracked context.

## Maintenance

- Changing an invariant/validation → grep the tests that build that entity/request and give them valid data.
- When a shared helper changes contract, update dependent tests to the **new** contract; never weaken correct code
  to satisfy a stale test.
- Fix nullable warnings in test files you touch.
- Run commands: `dotnet test <Project>.csproj --logger "console;verbosity=minimal"`; single test:
  `dotnet test --filter "FullyQualifiedName~CreateAsyncShouldReturnFailure"`.
