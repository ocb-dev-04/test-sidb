using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Aplicacion.Features.Users.GetUsers;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Persistence.Repositories;

internal sealed class UserReadRepository(ApplicationDbContext context) : IUserReadRepository
{
    public async Task<PagedResult<UserResponse>> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        IQueryable<User> users = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            users = users.Where(user => user.FullName.Contains(term) || user.Email.Contains(term));
        }

        if (query.Role is { } role)
        {
            users = users.Where(user => user.Role == role);
        }

        if (query.IsActive is { } isActive)
        {
            users = users.Where(user => user.IsActive == isActive);
        }

        int totalCount = await users.CountAsync(cancellationToken);
        List<UserResponse> items = await users
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Skip(Pagination.Skip(query.Page, query.PageSize))
            .Take(query.PageSize)
            .Select(user => new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedOnUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<UserLookupResponse>> GetAssignableAsync(CancellationToken cancellationToken)
        => await context.Users
            .AsNoTracking()
            .Where(user => user.IsActive && (user.Role == UserRole.Administrator || user.Role == UserRole.Analyst))
            .OrderBy(user => user.FullName)
            .Select(user => new UserLookupResponse(user.Id, user.FullName, user.Role))
            .ToListAsync(cancellationToken);
}

internal sealed class NotificationReadRepository(ApplicationDbContext context) : INotificationReadRepository
{
    public async Task<PagedResult<NotificationResponse>> GetInAppForRecipientAsync(
        Guid recipientUserId,
        bool onlyUnread,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Notification> notifications = InAppFor(recipientUserId);
        if (onlyUnread)
        {
            notifications = notifications.Where(notification => notification.ReadOnUtc == null);
        }

        int totalCount = await notifications.CountAsync(cancellationToken);
        List<NotificationResponse> items = await notifications
            .OrderByDescending(notification => notification.CreatedOnUtc)
            .ThenBy(notification => notification.Id)
            .Skip(Pagination.Skip(page, pageSize))
            .Take(pageSize)
            .Select(notification => new NotificationResponse(
                notification.Id,
                notification.ServiceRequestId,
                notification.Type,
                notification.Subject,
                notification.Message,
                notification.CreatedOnUtc,
                notification.ReadOnUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(items, page, pageSize, totalCount);
    }

    public Task<int> CountUnreadInAppAsync(Guid recipientUserId, CancellationToken cancellationToken)
        => InAppFor(recipientUserId).CountAsync(notification => notification.ReadOnUtc == null, cancellationToken);

    private IQueryable<Notification> InAppFor(Guid recipientUserId)
        => context.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == recipientUserId && notification.Channel == NotificationChannel.InApp);
}
