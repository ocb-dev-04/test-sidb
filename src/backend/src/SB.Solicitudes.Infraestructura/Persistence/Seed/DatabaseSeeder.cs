using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Persistence.Seed;

/// <summary>
/// Carga datos de ejemplo al arrancar solo en las tablas que estén vacías; nunca sobrescribe datos existentes.
/// Las fechas son relativas al día de arranque para que el tablero (vencidas, abiertas) sea representativo.
/// </summary>
internal sealed class DatabaseSeeder(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IServiceRequestCodeGenerator codeGenerator,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    public const string DEMO_PASSWORD = "Demo2026*";
    public const string ADMINISTRATOR_EMAIL = "admin@solicitudes.local";
    public const string FIRST_ANALYST_EMAIL = "analista1@solicitudes.local";
    public const string SECOND_ANALYST_EMAIL = "analista2@solicitudes.local";
    public const string FIRST_REQUESTER_EMAIL = "solicitante1@solicitudes.local";
    public const string SECOND_REQUESTER_EMAIL = "solicitante2@solicitudes.local";

    private const int SAMPLE_CREATION_HOUR = 13;
    private const int ASSIGNMENT_DELAY_MINUTES = 20;
    private const int PUBLIC_COMMENT_DELAY_MINUTES = 30;
    private const int INTERNAL_COMMENT_DELAY_MINUTES = 45;

    private const string TECHNOLOGY_AREA = "Tecnología de la Información";
    private const string HUMAN_RESOURCES_AREA = "Recursos Humanos";
    private const string FINANCE_AREA = "Finanzas";
    private const string SUPERVISION_AREA = "Supervisión Bancaria";
    private const string LEGAL_AREA = "Consultoría Jurídica";
    private const string GENERAL_SERVICES_AREA = "Servicios Generales";

    private const string TECHNICAL_SUPPORT_TYPE = "Soporte técnico";
    private const string SYSTEM_ACCESS_TYPE = "Acceso a sistemas";
    private const string DEVELOPMENT_TYPE = "Requerimiento de desarrollo";
    private const string SECURITY_INCIDENT_TYPE = "Incidente de seguridad";
    private const string HARDWARE_TYPE = "Equipos y periféricos";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        int areas = await SeedAreasAsync(cancellationToken);
        int requestTypes = await SeedRequestTypesAsync(cancellationToken);
        int users = await SeedUsersAsync(cancellationToken);
        int serviceRequests = await SeedServiceRequestsAsync(cancellationToken);

        InfrastructureLogMessages.DatabaseSeeded(logger, areas, requestTypes, users, serviceRequests);
    }

    private async Task<int> SeedAreasAsync(CancellationToken cancellationToken)
    {
        if (await context.Areas.AnyAsync(cancellationToken))
        {
            return 0;
        }

        string[] names = [TECHNOLOGY_AREA, HUMAN_RESOURCES_AREA, FINANCE_AREA, SUPERVISION_AREA, LEGAL_AREA, GENERAL_SERVICES_AREA];
        context.Areas.AddRange(names.Select(Area.Create));
        await context.SaveChangesAsync(cancellationToken);
        return names.Length;
    }

    private async Task<int> SeedRequestTypesAsync(CancellationToken cancellationToken)
    {
        if (await context.RequestTypes.AnyAsync(cancellationToken))
        {
            return 0;
        }

        RequestType[] requestTypes =
        [
            RequestType.Create(TECHNICAL_SUPPORT_TYPE, "Fallas de software, correo o herramientas de oficina."),
            RequestType.Create(SYSTEM_ACCESS_TYPE, "Altas, bajas y cambios de permisos en sistemas institucionales."),
            RequestType.Create(DEVELOPMENT_TYPE, "Nuevas funcionalidades, reportes o ajustes a aplicaciones."),
            RequestType.Create(SECURITY_INCIDENT_TYPE, "Correos sospechosos, malware o accesos no autorizados."),
            RequestType.Create(HARDWARE_TYPE, "Computadoras, impresoras y otros dispositivos."),
        ];
        context.RequestTypes.AddRange(requestTypes);
        await context.SaveChangesAsync(cancellationToken);
        return requestTypes.Length;
    }

    private async Task<int> SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            return 0;
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        string passwordHash = passwordHasher.Hash(DEMO_PASSWORD);
        User[] users =
        [
            User.Create("Administrador del Sistema", ADMINISTRATOR_EMAIL, passwordHash, UserRole.Administrator, utcNow),
            User.Create("María Fernández", FIRST_ANALYST_EMAIL, passwordHash, UserRole.Analyst, utcNow),
            User.Create("José Martínez", SECOND_ANALYST_EMAIL, passwordHash, UserRole.Analyst, utcNow),
            User.Create("Ana Rodríguez", FIRST_REQUESTER_EMAIL, passwordHash, UserRole.Requester, utcNow),
            User.Create("Carlos Pérez", SECOND_REQUESTER_EMAIL, passwordHash, UserRole.Requester, utcNow),
        ];
        context.Users.AddRange(users);
        await context.SaveChangesAsync(cancellationToken);
        return users.Length;
    }

    private async Task<int> SeedServiceRequestsAsync(CancellationToken cancellationToken)
    {
        if (await context.ServiceRequests.AnyAsync(cancellationToken))
        {
            return 0;
        }

        Dictionary<string, User> users = await context.Users.ToDictionaryAsync(user => user.Email, cancellationToken);
        Dictionary<string, int> areas = await context.Areas.ToDictionaryAsync(area => area.Name, area => area.Id, cancellationToken);
        Dictionary<string, int> requestTypes = await context.RequestTypes.ToDictionaryAsync(requestType => requestType.Name, requestType => requestType.Id, cancellationToken);

        string[] requiredEmails = [ADMINISTRATOR_EMAIL, FIRST_ANALYST_EMAIL, SECOND_ANALYST_EMAIL, FIRST_REQUESTER_EMAIL, SECOND_REQUESTER_EMAIL];
        if (requiredEmails.Any(email => !users.ContainsKey(email)))
        {
            return 0;
        }

        DateTime today = timeProvider.GetUtcNow().UtcDateTime.Date;
        Guid administratorId = users[ADMINISTRATOR_EMAIL].Id;
        IReadOnlyList<SampleRequest> samples = BuildSamples();

        foreach (SampleRequest sample in samples)
        {
            DateTime createdOnUtc = today.AddDays(-sample.DaysAgo).AddHours(SAMPLE_CREATION_HOUR);
            string code = await codeGenerator.GenerateAsync(createdOnUtc.Year, cancellationToken);
            ServiceRequest serviceRequest = ServiceRequest.Create(
                code,
                sample.Title,
                sample.Description,
                sample.Priority,
                requestTypes[sample.RequestType],
                areas[sample.Area],
                users[sample.RequesterEmail].Id,
                sample.EvidenceReference,
                createdOnUtc);

            if (sample.AssigneeEmail is { } assigneeEmail)
            {
                User assignee = users[assigneeEmail];
                EnsureSuccess(serviceRequest.Assign(assignee.Id, administratorId, createdOnUtc.AddMinutes(ASSIGNMENT_DELAY_MINUTES)));
                serviceRequest.AddComment(assignee.Id, "Recibimos su solicitud y ya la estamos revisando.", CommentVisibility.Public, createdOnUtc.AddMinutes(PUBLIC_COMMENT_DELAY_MINUTES));
                serviceRequest.AddComment(assignee.Id, "Nota interna: validar con el supervisor del área antes de cerrar.", CommentVisibility.Internal, createdOnUtc.AddMinutes(INTERNAL_COMMENT_DELAY_MINUTES));

                foreach (SampleTransition transition in sample.Transitions)
                {
                    EnsureSuccess(serviceRequest.ChangeStatus(
                        transition.Status,
                        assignee.Id,
                        assignee.Role,
                        transition.Comment,
                        createdOnUtc.AddHours(transition.HoursAfterCreation)));
                }
            }

            // Los datos de ejemplo no deben disparar notificaciones ni correos simulados.
            serviceRequest.ClearDomainEvents();
            context.ServiceRequests.Add(serviceRequest);
        }

        await context.SaveChangesAsync(cancellationToken);
        return samples.Count;
    }

    private static void EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Invalid sample data: {result.Error.Code}");
        }
    }

    private static IReadOnlyList<SampleRequest> BuildSamples() =>
    [
        new(25, "No puedo acceder al correo institucional", "Desde esta mañana Outlook indica que la cuenta está bloqueada.",
            "https://intranet.local/capturas/correo-bloqueado.png", ServiceRequestPriority.High, TECHNICAL_SUPPORT_TYPE, FINANCE_AREA,
            FIRST_REQUESTER_EMAIL, FIRST_ANALYST_EMAIL,
            [
                new(ServiceRequestStatus.InAnalysis, 1, null),
                new(ServiceRequestStatus.InProgress, 2, null),
                new(ServiceRequestStatus.Resolved, 5, "Se desbloqueó la cuenta y se restableció la contraseña."),
                new(ServiceRequestStatus.Closed, 20, "La solicitante confirmó el acceso al correo."),
            ]),
        new(15, "Acceso al sistema de supervisión de entidades", "Requiero perfil de consulta en el sistema de supervisión para el nuevo analista del área.",
            null, ServiceRequestPriority.Medium, SYSTEM_ACCESS_TYPE, SUPERVISION_AREA, SECOND_REQUESTER_EMAIL, SECOND_ANALYST_EMAIL,
            [
                new(ServiceRequestStatus.InAnalysis, 2, null),
                new(ServiceRequestStatus.InProgress, 24, "Pendiente aprobación del supervisor directo."),
            ]),
        new(9, "Reporte mensual de cartera en Excel", "Automatizar el reporte mensual de cartera que hoy se arma manualmente.",
            null, ServiceRequestPriority.Low, DEVELOPMENT_TYPE, FINANCE_AREA, FIRST_REQUESTER_EMAIL, FIRST_ANALYST_EMAIL,
            [new(ServiceRequestStatus.InAnalysis, 3, null)]),
        new(7, "Correo sospechoso con archivo adjunto", "Recibí un correo que simula ser de un banco con un adjunto .zip.",
            "Asunto: Estado de cuenta pendiente", ServiceRequestPriority.Critical, SECURITY_INCIDENT_TYPE, SUPERVISION_AREA,
            SECOND_REQUESTER_EMAIL, SECOND_ANALYST_EMAIL,
            [
                new(ServiceRequestStatus.InAnalysis, 1, null),
                new(ServiceRequestStatus.Resolved, 4, "Correo bloqueado en la pasarela y remitente agregado a la lista negra."),
            ]),
        new(5, "Laptop no enciende", "La laptop asignada no enciende aun conectada al cargador.",
            null, ServiceRequestPriority.High, HARDWARE_TYPE, FINANCE_AREA, FIRST_REQUESTER_EMAIL, SECOND_ANALYST_EMAIL,
            [
                new(ServiceRequestStatus.InAnalysis, 1, null),
                new(ServiceRequestStatus.WaitingForRequester, 6, "Necesitamos el número de inventario del equipo."),
            ]),
        new(2, "Instalación de software estadístico", "Solicito instalar R y RStudio para análisis de riesgo.",
            null, ServiceRequestPriority.Medium, TECHNICAL_SUPPORT_TYPE, SUPERVISION_AREA, SECOND_REQUESTER_EMAIL, null, []),
        new(1, "Permisos de carpeta compartida", "Necesito permisos de escritura en la carpeta compartida de presupuesto.",
            null, ServiceRequestPriority.Medium, SYSTEM_ACCESS_TYPE, FINANCE_AREA, FIRST_REQUESTER_EMAIL, null, []),
        new(1, "Error al guardar el formulario de viáticos", "El formulario de viáticos muestra un error 500 al guardar.",
            "https://intranet.local/viaticos", ServiceRequestPriority.High, DEVELOPMENT_TYPE, HUMAN_RESOURCES_AREA,
            SECOND_REQUESTER_EMAIL, FIRST_ANALYST_EMAIL,
            [new(ServiceRequestStatus.InAnalysis, 2, null)]),
    ];

    private sealed record SampleTransition(ServiceRequestStatus Status, int HoursAfterCreation, string? Comment);

    private sealed record SampleRequest(
        int DaysAgo,
        string Title,
        string Description,
        string? EvidenceReference,
        ServiceRequestPriority Priority,
        string RequestType,
        string Area,
        string RequesterEmail,
        string? AssigneeEmail,
        SampleTransition[] Transitions);
}
