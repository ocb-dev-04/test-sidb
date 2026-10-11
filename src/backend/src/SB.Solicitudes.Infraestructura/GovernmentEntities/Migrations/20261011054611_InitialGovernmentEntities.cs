using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.Solicitudes.Infraestructura.GovernmentEntities.Migrations
{
    /// <inheritdoc />
    public partial class InitialGovernmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntidadesGubernamentales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PoderDelEstado = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sector = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntidadesGubernamentales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntidadesGubernamentales_Nombre",
                table: "EntidadesGubernamentales",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntidadesGubernamentales");
        }
    }
}
