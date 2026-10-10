using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Aplicacion.Tests.Features.GovernmentEntities;

public sealed class GovernmentEntityFeatureTests
{
    private const string EXECUTIVE_BRANCH = "Poder Ejecutivo";
    private const string DECENTRALIZED = "Organismo Descentralizado Funcionalmente";
    private const string EDUCATION_SECTOR = "Educación Superior, Ciencia y Tecnología";
    private const string CULTURE_SECTOR = "Cultura";
    private const string UNIVERSITY = "Universidad Autónoma de Santo Domingo";
    private const string ARCHIVE = "Archivo General de la Nación";
    private const string AQUARIUM = "Acuario Nacional";
    private const int SINGLE_ITEM_PAGE_SIZE = 1;
    private const int SECOND_PAGE = 2;

    private static readonly GovernmentEntity[] ENTITIES =
    [
        GovernmentEntity.Restore(1, UNIVERSITY, DECENTRALIZED, EXECUTIVE_BRANCH, EDUCATION_SECTOR),
        GovernmentEntity.Restore(2, ARCHIVE, DECENTRALIZED, EXECUTIVE_BRANCH, CULTURE_SECTOR),
        GovernmentEntity.Restore(3, AQUARIUM, DECENTRALIZED, EXECUTIVE_BRANCH, CULTURE_SECTOR),
    ];

    private readonly IGovernmentEntityStoreFactory _storeFactory = Substitute.For<IGovernmentEntityStoreFactory>();
    private readonly IGovernmentEntityReader _reader = Substitute.For<IGovernmentEntityReader>();

    public GovernmentEntityFeatureTests()
    {
        _reader.GetAllAsync(Arg.Any<CancellationToken>()).Returns(ENTITIES);
        _storeFactory.GetReader(Arg.Any<GovernmentEntityDataSource>()).Returns(Result.Success(_reader));
    }

    [Theory]
    [InlineData("autonoma", UNIVERSITY)]
    [InlineData("GENERAL DE LA NACIÓN", ARCHIVE)]
    [InlineData("acuario", AQUARIUM)]
    public void SearchShouldIgnoreCaseAndAccents(string term, string expectedName)
    {
        PagedResult<GovernmentEntityResponse> page = GovernmentEntitySearch.Apply(ENTITIES, new GetGovernmentEntitiesQuery { Search = term });

        page.Items.ShouldHaveSingleItem().Name.ShouldBe(expectedName);
    }

    [Fact]
    public void SearchShouldFilterBySectorAndPaginateSortedByName()
    {
        GetGovernmentEntitiesQuery query = new() { Sector = "cultura", PageSize = SINGLE_ITEM_PAGE_SIZE, Page = SECOND_PAGE };

        PagedResult<GovernmentEntityResponse> page = GovernmentEntitySearch.Apply(ENTITIES, query);

        int cultureEntitiesCount = ENTITIES.Count(entity => entity.Sector == CULTURE_SECTOR);
        page.TotalCount.ShouldBe(cultureEntitiesCount);
        page.TotalPages.ShouldBe(cultureEntitiesCount);
        page.Items.ShouldHaveSingleItem().Name.ShouldBe(ARCHIVE);
    }

    [Fact]
    public async Task CreateShouldRejectDuplicateNamesIgnoringAccents()
    {
        _storeFactory.GetWriter(Arg.Any<GovernmentEntityDataSource>()).Returns(Result.Success(Substitute.For<IGovernmentEntityWriter>()));
        CreateGovernmentEntityCommandHandler handler = new(_storeFactory, Options.Create(new GovernmentEntitiesOptions()));
        CreateGovernmentEntityCommand command = new("ARCHIVO GENERAL DE LA NACION", DECENTRALIZED, EXECUTIVE_BRANCH, CULTURE_SECTOR, null);

        Result<GovernmentEntityResponse> result = await handler.HandleAsync(command, CancellationToken.None);

        result.Error.ShouldBe(GovernmentEntityErrors.NAME_IN_USE);
    }

    [Fact]
    public async Task ImportShouldFailWhenDestinationIsReadOnly()
    {
        _storeFactory.GetWriter(GovernmentEntityDataSource.Excel).Returns(Result.Failure<IGovernmentEntityWriter>(GovernmentEntityErrors.SOURCE_READ_ONLY));
        ImportGovernmentEntitiesCommandHandler handler = new(_storeFactory, NullLogger<ImportGovernmentEntitiesCommandHandler>.Instance);

        Result<GovernmentEntityImportResponse> result = await handler.HandleAsync(
            new ImportGovernmentEntitiesCommand(GovernmentEntityDataSource.TextFile, GovernmentEntityDataSource.Excel),
            CancellationToken.None);

        result.Error.ShouldBe(GovernmentEntityErrors.SOURCE_READ_ONLY);
    }

    [Fact]
    public async Task ImportShouldReplaceDestinationWithSourceEntities()
    {
        IGovernmentEntityWriter writer = Substitute.For<IGovernmentEntityWriter>();
        _storeFactory.GetWriter(GovernmentEntityDataSource.Sqlite).Returns(Result.Success(writer));
        ImportGovernmentEntitiesCommandHandler handler = new(_storeFactory, NullLogger<ImportGovernmentEntitiesCommandHandler>.Instance);

        Result<GovernmentEntityImportResponse> result = await handler.HandleAsync(
            new ImportGovernmentEntitiesCommand(GovernmentEntityDataSource.Excel, GovernmentEntityDataSource.Sqlite),
            CancellationToken.None);

        result.Value.ImportedCount.ShouldBe(ENTITIES.Length);
        await writer.Received(1).ReplaceAllAsync(ENTITIES, Arg.Any<CancellationToken>());
    }
}
