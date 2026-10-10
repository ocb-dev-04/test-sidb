using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SB.Solicitudes.Infraestructura.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Activa", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Tecnología de la Información" },
                    { 2, true, "Recursos Humanos" },
                    { 3, true, "Finanzas" },
                    { 4, true, "Supervisión Bancaria" },
                    { 5, true, "Consultoría Jurídica" },
                    { 6, true, "Servicios Generales" }
                });

            migrationBuilder.InsertData(
                table: "ContadoresSolicitud",
                columns: new[] { "Anio", "UltimoNumero" },
                values: new object[] { 2026, 8 });

            migrationBuilder.InsertData(
                table: "TiposSolicitud",
                columns: new[] { "Id", "Descripcion", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, "Fallas de software, correo o herramientas de oficina.", true, "Soporte técnico" },
                    { 2, "Altas, bajas y cambios de permisos en sistemas institucionales.", true, "Acceso a sistemas" },
                    { 3, "Nuevas funcionalidades, reportes o ajustes a aplicaciones.", true, "Requerimiento de desarrollo" },
                    { 4, "Correos sospechosos, malware o accesos no autorizados.", true, "Incidente de seguridad" },
                    { 5, "Computadoras, impresoras y otros dispositivos.", true, "Equipos y periféricos" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "FechaCreacion", "Correo", "NombreCompleto", "Activo", "ClaveHash", "Rol" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000001"), new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc), "admin@solicitudes.local", "Administrador del Sistema", true, "PBKDF2-SHA256.100000.+9YkaJuGlrd4t4HrEysALQ==.I5f5qXFwU9IHlmzqYETatOxi5vYzLgqQ+UJ3xrVoaK4=", "Administrator" },
                    { new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc), "analista1@solicitudes.local", "María Fernández", true, "PBKDF2-SHA256.100000.deqNkOcer45y2xzt1cDhGg==.eqP1WaWO78h70d3eK2c/M7ym2ZGAWWmlTEPEYiLoVZc=", "Analyst" },
                    { new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc), "analista2@solicitudes.local", "José Martínez", true, "PBKDF2-SHA256.100000.u8erlCmOzpORwWZi/JDJxw==.ojn6Mwnmak6NQcuF6eWd57RdvsZQ+o4WqTLqfXSyeUQ=", "Analyst" },
                    { new Guid("00000004-0000-0000-0000-000000000001"), new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc), "solicitante1@solicitudes.local", "Ana Rodríguez", true, "PBKDF2-SHA256.100000.RyQJhmuqJ62Hs7/iul9gdA==.+Hft5Dwo9lFoA4NbEnNoiRtukL6oxwL2t+w/t+XaGHE=", "Requester" },
                    { new Guid("00000005-0000-0000-0000-000000000001"), new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc), "solicitante2@solicitudes.local", "Carlos Pérez", true, "PBKDF2-SHA256.100000.SlmZNl8iBtMxVVnRXZy1+A==.6JGruT5IMLeerH2H+VdBjvUG1FTnpJqPcm15eAwc6h8=", "Requester" }
                });

            migrationBuilder.InsertData(
                table: "Solicitudes",
                columns: new[] { "Id", "AreaId", "ResponsableId", "FechaCierre", "Codigo", "FechaCreacion", "Descripcion", "FechaCompromiso", "ReferenciaEvidencia", "Prioridad", "TipoSolicitudId", "SolicitanteId", "ComentarioResolucion", "FechaResolucion", "Estado", "Titulo", "FechaActualizacion" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000002"), 3, new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 16, 9, 0, 0, 0, DateTimeKind.Utc), "SOL-2026-0001", new DateTime(2026, 9, 15, 13, 0, 0, 0, DateTimeKind.Utc), "Desde esta mañana Outlook indica que la cuenta está bloqueada.", new DateTime(2026, 9, 16, 13, 0, 0, 0, DateTimeKind.Utc), "https://intranet.local/capturas/correo-bloqueado.png", "High", 1, new Guid("00000004-0000-0000-0000-000000000001"), "La solicitante confirmó el acceso al correo.", new DateTime(2026, 9, 15, 18, 0, 0, 0, DateTimeKind.Utc), "Closed", "No puedo acceder al correo institucional", new DateTime(2026, 9, 16, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000002-0000-0000-0000-000000000002"), 4, new Guid("00000003-0000-0000-0000-000000000001"), null, "SOL-2026-0002", new DateTime(2026, 9, 25, 14, 30, 0, 0, DateTimeKind.Utc), "Requiero perfil de consulta en el sistema de supervisión para el nuevo analista del área.", new DateTime(2026, 9, 28, 14, 30, 0, 0, DateTimeKind.Utc), null, "Medium", 2, new Guid("00000005-0000-0000-0000-000000000001"), null, null, "InProgress", "Acceso al sistema de supervisión de entidades", new DateTime(2026, 9, 26, 14, 30, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000003-0000-0000-0000-000000000002"), 3, new Guid("00000002-0000-0000-0000-000000000001"), null, "SOL-2026-0003", new DateTime(2026, 10, 1, 15, 0, 0, 0, DateTimeKind.Utc), "Automatizar el reporte mensual de cartera que hoy se arma manualmente.", new DateTime(2026, 10, 6, 15, 0, 0, 0, DateTimeKind.Utc), null, "Low", 3, new Guid("00000004-0000-0000-0000-000000000001"), null, null, "InAnalysis", "Reporte mensual de cartera en Excel", new DateTime(2026, 10, 1, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000004-0000-0000-0000-000000000002"), 4, new Guid("00000003-0000-0000-0000-000000000001"), null, "SOL-2026-0004", new DateTime(2026, 10, 3, 12, 15, 0, 0, DateTimeKind.Utc), "Recibí un correo que simula ser de un banco con un adjunto .zip.", new DateTime(2026, 10, 3, 20, 15, 0, 0, DateTimeKind.Utc), "Asunto: Estado de cuenta pendiente", "Critical", 4, new Guid("00000005-0000-0000-0000-000000000001"), "Correo bloqueado en la pasarela y remitente agregado a la lista negra.", new DateTime(2026, 10, 3, 16, 15, 0, 0, DateTimeKind.Utc), "Resolved", "Correo sospechoso con archivo adjunto", new DateTime(2026, 10, 3, 16, 15, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000005-0000-0000-0000-000000000002"), 3, new Guid("00000003-0000-0000-0000-000000000001"), null, "SOL-2026-0005", new DateTime(2026, 10, 5, 13, 45, 0, 0, DateTimeKind.Utc), "La laptop asignada no enciende aun conectada al cargador.", new DateTime(2026, 10, 6, 13, 45, 0, 0, DateTimeKind.Utc), null, "High", 5, new Guid("00000004-0000-0000-0000-000000000001"), null, null, "WaitingForRequester", "Laptop no enciende", new DateTime(2026, 10, 5, 19, 45, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000006-0000-0000-0000-000000000002"), 4, null, null, "SOL-2026-0006", new DateTime(2026, 10, 8, 16, 0, 0, 0, DateTimeKind.Utc), "Solicito instalar R y RStudio para análisis de riesgo.", new DateTime(2026, 10, 11, 16, 0, 0, 0, DateTimeKind.Utc), null, "Medium", 1, new Guid("00000005-0000-0000-0000-000000000001"), null, null, "Registered", "Instalación de software estadístico", null },
                    { new Guid("00000007-0000-0000-0000-000000000002"), 3, null, null, "SOL-2026-0007", new DateTime(2026, 10, 9, 13, 20, 0, 0, DateTimeKind.Utc), "Necesito permisos de escritura en la carpeta compartida de presupuesto.", new DateTime(2026, 10, 12, 13, 20, 0, 0, DateTimeKind.Utc), null, "Medium", 2, new Guid("00000004-0000-0000-0000-000000000001"), null, null, "Registered", "Permisos de carpeta compartida", null },
                    { new Guid("00000008-0000-0000-0000-000000000002"), 2, new Guid("00000002-0000-0000-0000-000000000001"), null, "SOL-2026-0008", new DateTime(2026, 10, 9, 18, 10, 0, 0, DateTimeKind.Utc), "El formulario de viáticos muestra un error 500 al guardar.", new DateTime(2026, 10, 10, 18, 10, 0, 0, DateTimeKind.Utc), "https://intranet.local/viaticos", "High", 3, new Guid("00000005-0000-0000-0000-000000000001"), null, null, "InAnalysis", "Error al guardar el formulario de viáticos", new DateTime(2026, 10, 9, 20, 10, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Comentarios",
                columns: new[] { "Id", "UsuarioId", "Fecha", "SolicitudId", "Texto", "Visibilidad" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 13, 30, 0, 0, DateTimeKind.Utc), new Guid("00000001-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("00000002-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 13, 45, 0, 0, DateTimeKind.Utc), new Guid("00000001-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" },
                    { new Guid("00000003-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 9, 25, 15, 0, 0, 0, DateTimeKind.Utc), new Guid("00000002-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("00000004-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 9, 25, 15, 15, 0, 0, DateTimeKind.Utc), new Guid("00000002-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" },
                    { new Guid("00000005-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 1, 15, 30, 0, 0, DateTimeKind.Utc), new Guid("00000003-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("00000006-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 1, 15, 45, 0, 0, DateTimeKind.Utc), new Guid("00000003-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" },
                    { new Guid("00000007-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 12, 45, 0, 0, DateTimeKind.Utc), new Guid("00000004-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("00000008-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 13, 0, 0, 0, DateTimeKind.Utc), new Guid("00000004-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" },
                    { new Guid("00000009-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 5, 14, 15, 0, 0, DateTimeKind.Utc), new Guid("00000005-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("0000000a-0000-0000-0000-000000000004"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 5, 14, 30, 0, 0, DateTimeKind.Utc), new Guid("00000005-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" },
                    { new Guid("0000000b-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 9, 18, 40, 0, 0, DateTimeKind.Utc), new Guid("00000008-0000-0000-0000-000000000002"), "Recibimos su solicitud y ya la estamos revisando.", "Public" },
                    { new Guid("0000000c-0000-0000-0000-000000000004"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 9, 18, 55, 0, 0, DateTimeKind.Utc), new Guid("00000008-0000-0000-0000-000000000002"), "Nota interna: validar con el supervisor del área antes de cerrar.", "Internal" }
                });

            migrationBuilder.InsertData(
                table: "HistorialEstados",
                columns: new[] { "Id", "UsuarioId", "Fecha", "Comentario", "EstadoNuevo", "EstadoAnterior", "SolicitudId" },
                values: new object[,]
                {
                    { new Guid("00000001-0000-0000-0000-000000000003"), new Guid("00000004-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 13, 0, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000001-0000-0000-0000-000000000002") },
                    { new Guid("00000002-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 14, 0, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000001-0000-0000-0000-000000000002") },
                    { new Guid("00000003-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 15, 0, 0, 0, DateTimeKind.Utc), null, "InProgress", "InAnalysis", new Guid("00000001-0000-0000-0000-000000000002") },
                    { new Guid("00000004-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 15, 18, 0, 0, 0, DateTimeKind.Utc), "Se desbloqueó la cuenta y se restableció la contraseña.", "Resolved", "InProgress", new Guid("00000001-0000-0000-0000-000000000002") },
                    { new Guid("00000005-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 9, 16, 9, 0, 0, 0, DateTimeKind.Utc), "La solicitante confirmó el acceso al correo.", "Closed", "Resolved", new Guid("00000001-0000-0000-0000-000000000002") },
                    { new Guid("00000006-0000-0000-0000-000000000003"), new Guid("00000005-0000-0000-0000-000000000001"), new DateTime(2026, 9, 25, 14, 30, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000002-0000-0000-0000-000000000002") },
                    { new Guid("00000007-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 9, 25, 16, 30, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000002-0000-0000-0000-000000000002") },
                    { new Guid("00000008-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 9, 26, 14, 30, 0, 0, DateTimeKind.Utc), "Pendiente aprobación del supervisor directo.", "InProgress", "InAnalysis", new Guid("00000002-0000-0000-0000-000000000002") },
                    { new Guid("00000009-0000-0000-0000-000000000003"), new Guid("00000004-0000-0000-0000-000000000001"), new DateTime(2026, 10, 1, 15, 0, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000003-0000-0000-0000-000000000002") },
                    { new Guid("0000000a-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 1, 18, 0, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000003-0000-0000-0000-000000000002") },
                    { new Guid("0000000b-0000-0000-0000-000000000003"), new Guid("00000005-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 12, 15, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000004-0000-0000-0000-000000000002") },
                    { new Guid("0000000c-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 13, 15, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000004-0000-0000-0000-000000000002") },
                    { new Guid("0000000d-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 16, 15, 0, 0, DateTimeKind.Utc), "Correo bloqueado en la pasarela y remitente agregado a la lista negra.", "Resolved", "InAnalysis", new Guid("00000004-0000-0000-0000-000000000002") },
                    { new Guid("0000000e-0000-0000-0000-000000000003"), new Guid("00000004-0000-0000-0000-000000000001"), new DateTime(2026, 10, 5, 13, 45, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000005-0000-0000-0000-000000000002") },
                    { new Guid("0000000f-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 5, 14, 45, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000005-0000-0000-0000-000000000002") },
                    { new Guid("00000010-0000-0000-0000-000000000003"), new Guid("00000003-0000-0000-0000-000000000001"), new DateTime(2026, 10, 5, 19, 45, 0, 0, DateTimeKind.Utc), "Necesitamos el número de inventario del equipo.", "WaitingForRequester", "InAnalysis", new Guid("00000005-0000-0000-0000-000000000002") },
                    { new Guid("00000011-0000-0000-0000-000000000003"), new Guid("00000005-0000-0000-0000-000000000001"), new DateTime(2026, 10, 8, 16, 0, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000006-0000-0000-0000-000000000002") },
                    { new Guid("00000012-0000-0000-0000-000000000003"), new Guid("00000004-0000-0000-0000-000000000001"), new DateTime(2026, 10, 9, 13, 20, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000007-0000-0000-0000-000000000002") },
                    { new Guid("00000013-0000-0000-0000-000000000003"), new Guid("00000005-0000-0000-0000-000000000001"), new DateTime(2026, 10, 9, 18, 10, 0, 0, DateTimeKind.Utc), null, "Registered", null, new Guid("00000008-0000-0000-0000-000000000002") },
                    { new Guid("00000014-0000-0000-0000-000000000003"), new Guid("00000002-0000-0000-0000-000000000001"), new DateTime(2026, 10, 9, 20, 10, 0, 0, DateTimeKind.Utc), null, "InAnalysis", "Registered", new Guid("00000008-0000-0000-0000-000000000002") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000006-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000007-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000008-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("00000009-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("0000000a-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("0000000b-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Comentarios",
                keyColumn: "Id",
                keyValue: new Guid("0000000c-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "ContadoresSolicitud",
                keyColumn: "Anio",
                keyValue: 2026);

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000006-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000007-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000008-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000009-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000a-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000b-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000c-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000d-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000e-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("0000000f-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000010-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000011-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000012-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000013-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HistorialEstados",
                keyColumn: "Id",
                keyValue: new Guid("00000014-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000001-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000006-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000007-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: new Guid("00000008-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TiposSolicitud",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposSolicitud",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposSolicitud",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TiposSolicitud",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TiposSolicitud",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000002-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000003-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000004-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000005-0000-0000-0000-000000000001"));
        }
    }
}
