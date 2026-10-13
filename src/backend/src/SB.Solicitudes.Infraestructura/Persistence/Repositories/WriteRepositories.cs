using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Infraestructura.Persistence.Configurations;

namespace SB.Solicitudes.Infraestructura.Persistence.Repositories;

internal sealed class ServiceRequestRepository(ApplicationDbContext context) : IServiceRequestRepository
{
    // FindAsync consulta primero el change tracker: los handlers de eventos ven solicitudes aún no guardadas.
    public async Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.ServiceRequests.FindAsync([id], cancellationToken);

    // Detecta formularios desactualizados; la carrera entre esta lectura y el guardado la cubre el rowversion.
    public bool HasVersion(ServiceRequest serviceRequest, byte[] version)
        => context.Entry(serviceRequest).Property<byte[]>(ServiceRequestConfiguration.ROW_VERSION_PROPERTY).OriginalValue is byte[] current
            && current.AsSpan().SequenceEqual(version);

    public void Add(ServiceRequest serviceRequest) => context.ServiceRequests.Add(serviceRequest);
}

internal sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Users.FindAsync([id], cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => await context.Users.Where(user => ids.Contains(user.Id)).ToListAsync(cancellationToken);

    public void Add(User user) => context.Users.Add(user);
}

internal sealed class AreaRepository(ApplicationDbContext context) : IAreaRepository
{
    public async Task<Area?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await context.Areas.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Area>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => await context.Areas
            .AsNoTracking()
            .Where(area => includeInactive || area.IsActive)
            .OrderBy(area => area.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken)
        => context.Areas.AnyAsync(area => area.Name == name && area.Id != excludedId, cancellationToken);

    public void Add(Area area) => context.Areas.Add(area);
}

internal sealed class RequestTypeRepository(ApplicationDbContext context) : IRequestTypeRepository
{
    public async Task<RequestType?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await context.RequestTypes.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<RequestType>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
        => await context.RequestTypes
            .AsNoTracking()
            .Where(requestType => includeInactive || requestType.IsActive)
            .OrderBy(requestType => requestType.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken)
        => context.RequestTypes.AnyAsync(requestType => requestType.Name == name && requestType.Id != excludedId, cancellationToken);

    public void Add(RequestType requestType) => context.RequestTypes.Add(requestType);
}

internal sealed class NotificationRepository(ApplicationDbContext context) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Notifications.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetUnreadInAppForRecipientAsync(
        Guid recipientUserId,
        CancellationToken cancellationToken)
        => await context.Notifications
            .Where(notification => notification.RecipientUserId == recipientUserId)
            .Where(notification => notification.Channel == NotificationChannel.InApp)
            .Where(notification => notification.ReadOnUtc == null)
            .ToArrayAsync(cancellationToken);

    public void AddRange(IEnumerable<Notification> notifications) => context.Notifications.AddRange(notifications);
}
