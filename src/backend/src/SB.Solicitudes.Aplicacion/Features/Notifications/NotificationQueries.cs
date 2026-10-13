using FluentValidation;
using MediatR;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Notifications;

namespace SB.Solicitudes.Aplicacion.Features.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid? ServiceRequestId,
    NotificationType Type,
    string Subject,
    string Message,
    DateTime CreatedOnUtc,
    DateTime? ReadOnUtc);

public interface INotificationReadRepository
{
    Task<PagedResult<NotificationResponse>> GetInAppForRecipientAsync(
        Guid recipientUserId,
        bool onlyUnread,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountUnreadInAppAsync(Guid recipientUserId, CancellationToken cancellationToken);
}

public sealed record GetMyNotificationsQuery : IQuery<PagedResult<NotificationResponse>>
{
    public bool OnlyUnread { get; init; }

    public int Page { get; init; } = Pagination.DEFAULT_PAGE;

    public int PageSize { get; init; } = Pagination.DEFAULT_PAGE_SIZE;
}

public sealed record GetUnreadNotificationCountQuery : IQuery<int>;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<Unit>;

public sealed record MarkAllNotificationsAsReadCommand : ICommand<Unit>;

internal sealed class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
        => this.ValidatePagination(query => query.Page, query => query.PageSize);
}

internal sealed class GetMyNotificationsQueryHandler(INotificationReadRepository readRepository, ICurrentUser currentUser)
    : IQueryHandler<GetMyNotificationsQuery, PagedResult<NotificationResponse>>
{
    public async Task<Result<PagedResult<NotificationResponse>>> HandleAsync(GetMyNotificationsQuery request, CancellationToken cancellationToken)
        => await readRepository.GetInAppForRecipientAsync(
            currentUser.UserId, request.OnlyUnread, request.Page, request.PageSize, cancellationToken);
}

internal sealed class GetUnreadNotificationCountQueryHandler(INotificationReadRepository readRepository, ICurrentUser currentUser)
    : IQueryHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<Result<int>> HandleAsync(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        => await readRepository.CountUnreadInAppAsync(currentUser.UserId, cancellationToken);
}

internal sealed class MarkNotificationAsReadCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<MarkNotificationAsReadCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        Notification? notification = await notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null || notification.RecipientUserId != currentUser.UserId)
        {
            return NotificationErrors.NOT_FOUND;
        }

        notification.MarkAsRead(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

internal sealed class MarkAllNotificationsAsReadCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<MarkAllNotificationsAsReadCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Notification> unread
            = await notificationRepository.GetUnreadInAppForRecipientAsync(currentUser.UserId, cancellationToken);
        if (!unread.Any())
        {
            return Unit.Value;
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        foreach (Notification notification in unread)
        {
            notification.MarkAsRead(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
