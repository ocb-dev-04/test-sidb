using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Dominio.GovernmentEntities;
using SB.Solicitudes.Infraestructura.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.Tests.GovernmentEntities;

public sealed class SqliteGovernmentEntityStoreTests : IDisposable
{
    private const string CATEGORY = "Organismo Descentralizado Funcionalmente";
    private const string STATE_BRANCH = "Poder Ejecutivo";
    private const string SECTOR = "Cultura";
    private const string FIRST_NAME = "Archivo General de la Nación";
    private const string SECOND_NAME = "Biblioteca Nacional Pedro Henríquez Ureña";
    private const string RENAMED = "Biblioteca Nacional";
    private const int FIRST_ID = 1;
    private const int SECOND_ID = 2;

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

    [Fact]
    public async Task MigrationsShouldCreateSchemaWithHistory()
    {
        await using GovernmentEntitiesDbContext context = await CreateMigratedContextAsync();

        IEnumerable<string> applied = await context.Database.GetAppliedMigrationsAsync();
        IEnumerable<string> pending = await context.Database.GetPendingMigrationsAsync();

        applied.ShouldNotBeEmpty();
        pending.ShouldBeEmpty();
    }

    [Fact]
    public async Task StoreShouldReplaceAddUpdateAndDelete()
    {
        await using GovernmentEntitiesDbContext context = await CreateMigratedContextAsync();
        SqliteGovernmentEntityStore store = new(context);
        await store.ReplaceAllAsync(
            [GovernmentEntity.Restore(FIRST_ID, FIRST_NAME, CATEGORY, STATE_BRANCH, SECTOR)],
            CancellationToken.None);

        GovernmentEntity created = await store.AddAsync(GovernmentEntity.Create(SECOND_NAME, CATEGORY, STATE_BRANCH, SECTOR), CancellationToken.None);
        created.Update(RENAMED, CATEGORY, STATE_BRANCH, SECTOR);
        bool updated = await store.UpdateAsync(created, CancellationToken.None);
        bool deleted = await store.DeleteAsync(FIRST_ID, CancellationToken.None);
        IReadOnlyList<GovernmentEntity> remaining = await store.GetAllAsync(CancellationToken.None);

        created.Id.ShouldBe(SECOND_ID);
        updated.ShouldBeTrue();
        deleted.ShouldBeTrue();
        remaining.ShouldHaveSingleItem().Name.ShouldBe(RENAMED);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }

    private async Task<GovernmentEntitiesDbContext> CreateMigratedContextAsync()
    {
        GovernmentEntitiesDbContext context = new(new DbContextOptionsBuilder<GovernmentEntitiesDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options);
        await context.Database.MigrateAsync();
        return context;
    }
}
