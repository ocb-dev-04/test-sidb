namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

public sealed class GovernmentEntityStorageOptions
{
    public const string SECTION_NAME = "GovernmentEntities";
    public const string SQLITE_CONNECTION_STRING_NAME = "GovernmentEntitiesSqlite";

    public string TextFilePath { get; set; } = "Data/entidades-gubernamentales.txt";

    public string ExcelFilePath { get; set; } = "Data/ListaEntidadesGubernamentales.xlsx";

    public bool InitializeStoresOnStartup { get; set; } = true;
}
