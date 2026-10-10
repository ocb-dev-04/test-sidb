using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Infraestructura.Persistence;

/// <summary>
/// Consecutivo anual atómico: MERGE con HOLDLOCK evita que dos solicitudes simultáneas reciban el mismo número.
/// </summary>
internal sealed class ServiceRequestCodeGenerator(ApplicationDbContext context) : IServiceRequestCodeGenerator
{
    private const string YEAR_PARAMETER = "@year";

    private const string NEXT_NUMBER_SQL = """
        MERGE [ContadoresSolicitud] WITH (HOLDLOCK) AS target
        USING (SELECT @year AS [Anio]) AS source
            ON target.[Anio] = source.[Anio]
        WHEN MATCHED THEN
            UPDATE SET target.[UltimoNumero] = target.[UltimoNumero] + 1
        WHEN NOT MATCHED THEN
            INSERT ([Anio], [UltimoNumero]) VALUES (source.[Anio], 1)
        OUTPUT inserted.[UltimoNumero];
        """;

    public async Task<string> GenerateAsync(int year, CancellationToken cancellationToken)
    {
        DbConnection connection = context.Database.GetDbConnection();
        bool openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = NEXT_NUMBER_SQL;
            command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

            DbParameter yearParameter = command.CreateParameter();
            yearParameter.ParameterName = YEAR_PARAMETER;
            yearParameter.DbType = DbType.Int32;
            yearParameter.Value = year;
            command.Parameters.Add(yearParameter);

            object? nextNumber = await command.ExecuteScalarAsync(cancellationToken);
            return ServiceRequestCode.Format(year, Convert.ToInt32(nextNumber, CultureInfo.InvariantCulture));
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }
}
