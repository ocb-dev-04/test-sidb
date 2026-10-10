using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;
using SB.Solicitudes.Infraestructura.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.Tests.GovernmentEntities;

public sealed class ExcelAndFactoryTests
{
    private const int EXPECTED_ENTITY_COUNT = 181;
    private const string FIRST_ENTITY_NAME = "Acuario Nacional";
    private const string FIRST_ENTITY_SECTOR = "Medio Ambiente y Recursos Naturales";
    private const string EXCEL_RELATIVE_PATH = "TestData/ListaEntidadesGubernamentales.xlsx";

    [Fact]
    public async Task ExcelReaderShouldReadEveryRowByHeaderName()
    {
        ExcelGovernmentEntityReader reader = new(Options.Create(new GovernmentEntityStorageOptions
        {
            ExcelFilePath = Path.Combine(AppContext.BaseDirectory, EXCEL_RELATIVE_PATH),
        }));

        IReadOnlyList<GovernmentEntity> entities = await reader.GetAllAsync(CancellationToken.None);

        entities.Count.ShouldBe(EXPECTED_ENTITY_COUNT);
        entities[0].Id.ShouldBe(1);
        entities[0].Name.ShouldBe(FIRST_ENTITY_NAME);
        entities[0].Sector.ShouldBe(FIRST_ENTITY_SECTOR);
        entities.ShouldAllBe(entity => !string.IsNullOrWhiteSpace(entity.Category));
    }

    [Fact]
    public void FactoryShouldExposeExcelAsReadOnlyAndTextFileAsWritable()
    {
        ServiceCollection services = new();
        IGovernmentEntityReader excelReader = new ExcelGovernmentEntityReader(Options.Create(new GovernmentEntityStorageOptions()));
        TextFileGovernmentEntityStore textStore = new(Options.Create(new GovernmentEntityStorageOptions()));
        services.AddKeyedSingleton(GovernmentEntityDataSource.Excel, excelReader);
        services.AddKeyedSingleton<IGovernmentEntityReader>(GovernmentEntityDataSource.TextFile, textStore);
        services.AddKeyedSingleton<IGovernmentEntityWriter>(GovernmentEntityDataSource.TextFile, textStore);
        using ServiceProvider provider = services.BuildServiceProvider();
        GovernmentEntityStoreFactory factory = new(provider);

        Result<IGovernmentEntityWriter> excelWriter = factory.GetWriter(GovernmentEntityDataSource.Excel);
        Result<IGovernmentEntityWriter> sqliteWriter = factory.GetWriter(GovernmentEntityDataSource.Sqlite);

        factory.IsWritable(GovernmentEntityDataSource.TextFile).ShouldBeTrue();
        factory.GetReader(GovernmentEntityDataSource.Excel).IsSuccess.ShouldBeTrue();
        excelWriter.Error.ShouldBe(GovernmentEntityErrors.SOURCE_READ_ONLY);
        sqliteWriter.Error.ShouldBe(GovernmentEntityErrors.SOURCE_NOT_SUPPORTED);
    }
}
