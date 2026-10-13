using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IServiceRequestRepository
{
    Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    bool HasVersion(ServiceRequest serviceRequest, byte[] version);

    void Add(ServiceRequest serviceRequest);
}

public interface IServiceRequestCodeGenerator
{
    Task<string> GenerateAsync(int year, CancellationToken cancellationToken);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    void Add(User user);
}

public interface IAreaRepository
{
    Task<Area?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Area>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken);

    void Add(Area area);
}

public interface IRequestTypeRepository
{
    Task<RequestType?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<RequestType>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(string name, int? excludedId, CancellationToken cancellationToken);

    void Add(RequestType requestType);
}

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> GetUnreadInAppForRecipientAsync(Guid recipientUserId, CancellationToken cancellationToken);

    void AddRange(IEnumerable<Notification> notifications);
}
