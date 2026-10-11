using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

/// <summary>
/// Solo para `dotnet ef`: genera las migraciones de SQLite sin levantar la API.
/// </summary>
public sealed class GovernmentEntitiesDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GovernmentEntitiesDbContext>
{
    private const string DESIGN_TIME_CONNECTION_STRING = "Data Source=entidades-gubernamentales.design.db";

    public GovernmentEntitiesDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<GovernmentEntitiesDbContext>()
            .UseSqlite(DESIGN_TIME_CONNECTION_STRING)
            .Options);
}
