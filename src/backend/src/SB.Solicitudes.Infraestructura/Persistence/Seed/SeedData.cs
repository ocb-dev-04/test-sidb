using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Persistence.Seed;

/// <summary>
/// Datos de ejemplo deterministas (ids y fechas fijas) para que la migración de semilla sea reproducible.
/// La clave de todos los usuarios demo es "Demo2026*".
/// </summary>
internal static class SeedData
{
    private const int SEED_YEAR = 2026;
    private const byte USER_KIND = 1;
    private const byte REQUEST_KIND = 2;
    private const byte HISTORY_KIND = 3;
    private const byte COMMENT_KIND = 4;
    private const int GUID_TAIL_LENGTH = 8;
    private const int PUBLIC_COMMENT_DELAY_MINUTES = 30;
    private const int INTERNAL_COMMENT_DELAY_MINUTES = 45;

    private const int TECHNOLOGY_AREA = 1;
    private const int HUMAN_RESOURCES_AREA = 2;
    private const int FINANCE_AREA = 3;
    private const int SUPERVISION_AREA = 4;
    private const int LEGAL_AREA = 5;
    private const int GENERAL_SERVICES_AREA = 6;

    private const int TECHNICAL_SUPPORT_TYPE = 1;
    private const int SYSTEM_ACCESS_TYPE = 2;
    private const int DEVELOPMENT_TYPE = 3;
    private const int SECURITY_INCIDENT_TYPE = 4;
    private const int HARDWARE_TYPE = 5;

    public static readonly Guid ADMINISTRATOR_ID = SeedId(USER_KIND, 1);
    public static readonly Guid FIRST_ANALYST_ID = SeedId(USER_KIND, 2);
    public static readonly Guid SECOND_ANALYST_ID = SeedId(USER_KIND, 3);
    public static readonly Guid FIRST_REQUESTER_ID = SeedId(USER_KIND, 4);
    public static readonly Guid SECOND_REQUESTER_ID = SeedId(USER_KIND, 5);

    private static readonly DateTime USERS_CREATED_ON_UTC = Utc("2026-09-01T12:00:00Z");

    public static void ApplySeedData(this ModelBuilder modelBuilder)
    {
        SeedCatalogs(modelBuilder);
        SeedUsers(modelBuilder);
        SeedServiceRequests(modelBuilder);
    }

    private static DateTime Utc(string isoDate)
        => DateTime.Parse(isoDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private static Guid SeedId(byte kind, int number)
    {
        byte[] tail = new byte[GUID_TAIL_LENGTH];
        tail[^1] = kind;
        return new Guid(number, 0, 0, tail);
    }

    private static void SeedCatalogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>().HasData(
            new { Id = TECHNOLOGY_AREA, Name = "Tecnología de la Información", IsActive = true },
            new { Id = HUMAN_RESOURCES_AREA, Name = "Recursos Humanos", IsActive = true },
            new { Id = FINANCE_AREA, Name = "Finanzas", IsActive = true },
            new { Id = SUPERVISION_AREA, Name = "Supervisión Bancaria", IsActive = true },
            new { Id = LEGAL_AREA, Name = "Consultoría Jurídica", IsActive = true },
            new { Id = GENERAL_SERVICES_AREA, Name = "Servicios Generales", IsActive = true });

        modelBuilder.Entity<RequestType>().HasData(
            new { Id = TECHNICAL_SUPPORT_TYPE, Name = "Soporte técnico", Description = "Fallas de software, correo o herramientas de oficina.", IsActive = true },
            new { Id = SYSTEM_ACCESS_TYPE, Name = "Acceso a sistemas", Description = "Altas, bajas y cambios de permisos en sistemas institucionales.", IsActive = true },
            new { Id = DEVELOPMENT_TYPE, Name = "Requerimiento de desarrollo", Description = "Nuevas funcionalidades, reportes o ajustes a aplicaciones.", IsActive = true },
            new { Id = SECURITY_INCIDENT_TYPE, Name = "Incidente de seguridad", Description = "Correos sospechosos, malware o accesos no autorizados.", IsActive = true },
            new { Id = HARDWARE_TYPE, Name = "Equipos y periféricos", Description = "Computadoras, impresoras y otros dispositivos.", IsActive = true });
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
        => modelBuilder.Entity<User>().HasData(
            SeedUser(ADMINISTRATOR_ID, "Administrador del Sistema", "admin@solicitudes.local", UserRole.Administrator,
                "PBKDF2-SHA256.100000.+9YkaJuGlrd4t4HrEysALQ==.I5f5qXFwU9IHlmzqYETatOxi5vYzLgqQ+UJ3xrVoaK4="),
            SeedUser(FIRST_ANALYST_ID, "María Fernández", "analista1@solicitudes.local", UserRole.Analyst,
                "PBKDF2-SHA256.100000.deqNkOcer45y2xzt1cDhGg==.eqP1WaWO78h70d3eK2c/M7ym2ZGAWWmlTEPEYiLoVZc="),
            SeedUser(SECOND_ANALYST_ID, "José Martínez", "analista2@solicitudes.local", UserRole.Analyst,
                "PBKDF2-SHA256.100000.u8erlCmOzpORwWZi/JDJxw==.ojn6Mwnmak6NQcuF6eWd57RdvsZQ+o4WqTLqfXSyeUQ="),
            SeedUser(FIRST_REQUESTER_ID, "Ana Rodríguez", "solicitante1@solicitudes.local", UserRole.Requester,
                "PBKDF2-SHA256.100000.RyQJhmuqJ62Hs7/iul9gdA==.+Hft5Dwo9lFoA4NbEnNoiRtukL6oxwL2t+w/t+XaGHE="),
            SeedUser(SECOND_REQUESTER_ID, "Carlos Pérez", "solicitante2@solicitudes.local", UserRole.Requester,
                "PBKDF2-SHA256.100000.SlmZNl8iBtMxVVnRXZy1+A==.6JGruT5IMLeerH2H+VdBjvUG1FTnpJqPcm15eAwc6h8="));

    private static object SeedUser(Guid id, string fullName, string email, UserRole role, string passwordHash)
        => new
        {
            Id = id,
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedOnUtc = USERS_CREATED_ON_UTC,
        };

    private static void SeedServiceRequests(ModelBuilder modelBuilder)
    {
        SampleRequest[] samples =
        [
            new(1, "No puedo acceder al correo institucional", "Desde esta mañana Outlook indica que la cuenta está bloqueada.", "https://intranet.local/capturas/correo-bloqueado.png",
                ServiceRequestPriority.High, TECHNICAL_SUPPORT_TYPE, FINANCE_AREA, FIRST_REQUESTER_ID, FIRST_ANALYST_ID, Utc("2026-09-15T13:00:00Z"),
                [
                    new(ServiceRequestStatus.InAnalysis, FIRST_ANALYST_ID, 1, null),
                    new(ServiceRequestStatus.InProgress, FIRST_ANALYST_ID, 2, null),
                    new(ServiceRequestStatus.Resolved, FIRST_ANALYST_ID, 5, "Se desbloqueó la cuenta y se restableció la contraseña."),
                    new(ServiceRequestStatus.Closed, FIRST_ANALYST_ID, 20, "La solicitante confirmó el acceso al correo."),
                ]),
            new(2, "Acceso al sistema de supervisión de entidades", "Requiero perfil de consulta en el sistema de supervisión para el nuevo analista del área.", null,
                ServiceRequestPriority.Medium, SYSTEM_ACCESS_TYPE, SUPERVISION_AREA, SECOND_REQUESTER_ID, SECOND_ANALYST_ID, Utc("2026-09-25T14:30:00Z"),
                [
                    new(ServiceRequestStatus.InAnalysis, SECOND_ANALYST_ID, 2, null),
                    new(ServiceRequestStatus.InProgress, SECOND_ANALYST_ID, 24, "Pendiente aprobación del supervisor directo."),
                ]),
            new(3, "Reporte mensual de cartera en Excel", "Automatizar el reporte mensual de cartera que hoy se arma manualmente.", null,
                ServiceRequestPriority.Low, DEVELOPMENT_TYPE, FINANCE_AREA, FIRST_REQUESTER_ID, FIRST_ANALYST_ID, Utc("2026-10-01T15:00:00Z"),
                [new(ServiceRequestStatus.InAnalysis, FIRST_ANALYST_ID, 3, null)]),
            new(4, "Correo sospechoso con archivo adjunto", "Recibí un correo que simula ser de un banco con un adjunto .zip.", "Asunto: Estado de cuenta pendiente",
                ServiceRequestPriority.Critical, SECURITY_INCIDENT_TYPE, SUPERVISION_AREA, SECOND_REQUESTER_ID, SECOND_ANALYST_ID, Utc("2026-10-03T12:15:00Z"),
                [
                    new(ServiceRequestStatus.InAnalysis, SECOND_ANALYST_ID, 1, null),
                    new(ServiceRequestStatus.Resolved, SECOND_ANALYST_ID, 4, "Correo bloqueado en la pasarela y remitente agregado a la lista negra."),
                ]),
            new(5, "Laptop no enciende", "La laptop asignada no enciende aun conectada al cargador.", null,
                ServiceRequestPriority.High, HARDWARE_TYPE, FINANCE_AREA, FIRST_REQUESTER_ID, SECOND_ANALYST_ID, Utc("2026-10-05T13:45:00Z"),
                [
                    new(ServiceRequestStatus.InAnalysis, SECOND_ANALYST_ID, 1, null),
                    new(ServiceRequestStatus.WaitingForRequester, SECOND_ANALYST_ID, 6, "Necesitamos el número de inventario del equipo."),
                ]),
            new(6, "Instalación de software estadístico", "Solicito instalar R y RStudio para análisis de riesgo.", null,
                ServiceRequestPriority.Medium, TECHNICAL_SUPPORT_TYPE, SUPERVISION_AREA, SECOND_REQUESTER_ID, null, Utc("2026-10-08T16:00:00Z"), []),
            new(7, "Permisos de carpeta compartida", "Necesito permisos de escritura en la carpeta compartida de presupuesto.", null,
                ServiceRequestPriority.Medium, SYSTEM_ACCESS_TYPE, FINANCE_AREA, FIRST_REQUESTER_ID, null, Utc("2026-10-09T13:20:00Z"), []),
            new(8, "Error al guardar el formulario de viáticos", "El formulario de viáticos muestra un error 500 al guardar.", "https://intranet.local/viaticos",
                ServiceRequestPriority.High, DEVELOPMENT_TYPE, HUMAN_RESOURCES_AREA, SECOND_REQUESTER_ID, FIRST_ANALYST_ID, Utc("2026-10-09T18:10:00Z"),
                [new(ServiceRequestStatus.InAnalysis, FIRST_ANALYST_ID, 2, null)]),
        ];

        List<object> requests = [];
        List<object> histories = [];
        List<object> comments = [];
        int historyNumber = 0;
        int commentNumber = 0;

        foreach (SampleRequest sample in samples)
        {
            Guid requestId = SeedId(REQUEST_KIND, sample.Number);
            ServiceRequestStatus status = ServiceRequestStatus.Registered;
            DateTime lastChange = sample.CreatedOnUtc;
            DateTime? resolvedOn = null;
            DateTime? closedOn = null;
            string? resolutionComment = null;

            histories.Add(History(++historyNumber, requestId, null, ServiceRequestStatus.Registered, sample.RequesterId, sample.CreatedOnUtc, null));
            foreach (SampleTransition transition in sample.Transitions)
            {
                lastChange = sample.CreatedOnUtc.AddHours(transition.HoursAfterCreation);
                histories.Add(History(++historyNumber, requestId, status, transition.Status, transition.ChangedById, lastChange, transition.Comment));
                status = transition.Status;
                if (status is ServiceRequestStatus.Resolved or ServiceRequestStatus.Closed)
                {
                    resolvedOn ??= lastChange;
                    resolutionComment = transition.Comment;
                }

                closedOn = status == ServiceRequestStatus.Closed ? lastChange : null;
            }

            if (sample.AssigneeId is { } assigneeId)
            {
                comments.Add(new
                {
                    Id = SeedId(COMMENT_KIND, ++commentNumber),
                    ServiceRequestId = requestId,
                    AuthorId = assigneeId,
                    Text = "Recibimos su solicitud y ya la estamos revisando.",
                    Visibility = CommentVisibility.Public,
                    CreatedOnUtc = sample.CreatedOnUtc.AddMinutes(PUBLIC_COMMENT_DELAY_MINUTES),
                });
                comments.Add(new
                {
                    Id = SeedId(COMMENT_KIND, ++commentNumber),
                    ServiceRequestId = requestId,
                    AuthorId = assigneeId,
                    Text = "Nota interna: validar con el supervisor del área antes de cerrar.",
                    Visibility = CommentVisibility.Internal,
                    CreatedOnUtc = sample.CreatedOnUtc.AddMinutes(INTERNAL_COMMENT_DELAY_MINUTES),
                });
            }

            requests.Add(new
            {
                Id = requestId,
                Code = ServiceRequestCode.Format(SEED_YEAR, sample.Number),
                sample.Title,
                sample.Description,
                sample.EvidenceReference,
                sample.Priority,
                Status = status,
                sample.RequestTypeId,
                sample.AreaId,
                sample.RequesterId,
                sample.AssigneeId,
                sample.CreatedOnUtc,
                DueDateUtc = sample.CreatedOnUtc + PriorityServiceLevels.ResolutionTimeFor(sample.Priority),
                UpdatedOnUtc = sample.Transitions.Length == 0 ? (DateTime?)null : lastChange,
                ResolvedOnUtc = resolvedOn,
                ClosedOnUtc = closedOn,
                ResolutionComment = resolutionComment,
            });
        }

        modelBuilder.Entity<ServiceRequest>().HasData(requests);
        modelBuilder.Entity<StatusHistory>().HasData(histories);
        modelBuilder.Entity<Comment>().HasData(comments);
        modelBuilder.Entity<ServiceRequestCounter>().HasData(new ServiceRequestCounter { Year = SEED_YEAR, LastNumber = samples.Length });
    }

    private static object History(
        int number,
        Guid serviceRequestId,
        ServiceRequestStatus? previousStatus,
        ServiceRequestStatus newStatus,
        Guid changedById,
        DateTime changedOnUtc,
        string? comment)
        => new
        {
            Id = SeedId(HISTORY_KIND, number),
            ServiceRequestId = serviceRequestId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedById = changedById,
            ChangedOnUtc = changedOnUtc,
            Comment = comment,
        };

    private sealed record SampleTransition(ServiceRequestStatus Status, Guid ChangedById, int HoursAfterCreation, string? Comment);

    private sealed record SampleRequest(
        int Number,
        string Title,
        string Description,
        string? EvidenceReference,
        ServiceRequestPriority Priority,
        int RequestTypeId,
        int AreaId,
        Guid RequesterId,
        Guid? AssigneeId,
        DateTime CreatedOnUtc,
        SampleTransition[] Transitions);
}
