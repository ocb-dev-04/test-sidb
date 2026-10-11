using Microsoft.Extensions.Options;
using SB.Solicitudes.Dominio.GovernmentEntities;
using SB.Solicitudes.Infraestructura.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.Tests.GovernmentEntities;

public sealed class TextFileGovernmentEntityStoreTests : IDisposable
{
    private const string TRICKY_NAME = "Entidad | con \\ separadores, \"comillas\"\ny salto de línea";
    private const string QUOTED_NAME = "Jardín Botánico Nacional \"Dr. Rafael M. Moscoso\"";
    private const string CATEGORY ="Organismo Descentralizado Funcionalmente";
    private const string STATE_BRANCH = "Poder Ejecutivo";
    private const string SECTOR = "Industria, Comercio y MIPYMES";
    private const string UPDATED_NAME = "Entidad renombrada";
    private const string FILE_NAME = "entidades.txt";
    private const int CONCURRENT_WRITES = 20;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly TextFileGovernmentEntityStore _store;

    public TextFileGovernmentEntityStoreTests()
        => _store = new TextFileGovernmentEntityStore(Options.Create(new GovernmentEntityStorageOptions
        {
            TextFilePath = Path.Combine(_directory, FILE_NAME),
        }));

    [Fact]
    public async Task AddShouldAssignSequentialIdsAndRoundTripEscapedCharacters()
    {
        GovernmentEntity first = await _store.AddAsync(GovernmentEntity.Create(TRICKY_NAME, CATEGORY, STATE_BRANCH, SECTOR), CancellationToken.None);
        GovernmentEntity second = await _store.AddAsync(GovernmentEntity.Create(UPDATED_NAME, CATEGORY, STATE_BRANCH, SECTOR), CancellationToken.None);

        IReadOnlyList<GovernmentEntity> stored = await _store.GetAllAsync(CancellationToken.None);

        second.Id.ShouldBe(first.Id + 1);
        stored.Count.ShouldBe(2);
        stored[0].Name.ShouldBe(TRICKY_NAME);
        stored[0].Sector.ShouldBe(SECTOR);
        File.ReadAllLines(Path.Combine(_directory, FILE_NAME))[0].ShouldBe(TextFileGovernmentEntityStore.HEADER);
    }

    [Fact]
    public async Task GetAllShouldReadHandWrittenLinesWithQuotesInsideUnquotedFields()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllLinesAsync(
            Path.Combine(_directory, FILE_NAME),
            [TextFileGovernmentEntityStore.HEADER, $"1|{QUOTED_NAME}|{CATEGORY}|{STATE_BRANCH}|{SECTOR}"]);

        IReadOnlyList<GovernmentEntity> stored = await _store.GetAllAsync(CancellationToken.None);

        stored.ShouldHaveSingleItem().Name.ShouldBe(QUOTED_NAME);
    }

    [Fact]
    public async Task UpdateAndDeleteShouldReportWhetherTheEntityExisted()
    {
        GovernmentEntity created = await _store.AddAsync(GovernmentEntity.Create(TRICKY_NAME, CATEGORY, STATE_BRANCH, SECTOR), CancellationToken.None);
        created.Update(UPDATED_NAME, CATEGORY, STATE_BRANCH, SECTOR);

        bool updated = await _store.UpdateAsync(created, CancellationToken.None);
        bool deleted = await _store.DeleteAsync(created.Id, CancellationToken.None);
        bool deletedAgain = await _store.DeleteAsync(created.Id, CancellationToken.None);

        updated.ShouldBeTrue();
        deleted.ShouldBeTrue();
        deletedAgain.ShouldBeFalse();
        (await _store.GetAllAsync(CancellationToken.None)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ConcurrentAddsShouldNotLoseWritesOrDuplicateIds()
    {
        IEnumerable<Task<GovernmentEntity>> writes = Enumerable.Range(1, CONCURRENT_WRITES)
            .Select(number => _store.AddAsync(
                GovernmentEntity.Create($"Entidad {number}", CATEGORY, STATE_BRANCH, SECTOR),
                CancellationToken.None));

        GovernmentEntity[] created = await Task.WhenAll(writes);

        created.Select(entity => entity.Id).Distinct().Count().ShouldBe(CONCURRENT_WRITES);
        (await _store.GetAllAsync(CancellationToken.None)).Count.ShouldBe(CONCURRENT_WRITES);
    }

    public void Dispose()
    {
        _store.Dispose();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
