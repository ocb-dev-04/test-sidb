using MediatR;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Tests.Features.Notifications;

public sealed class MarkAllNotificationsAsReadCommandHandlerTests
{
    private const string SUBJECT = "Solicitud actualizada";
    private const string MESSAGE = "La solicitud cambió de estado.";

    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly MarkAllNotificationsAsReadCommandHandler _handler;

    public MarkAllNotificationsAsReadCommandHandlerTests()
        => _handler = new MarkAllNotificationsAsReadCommandHandler(
            _notificationRepository,
            _unitOfWork,
            new TestCurrentUser(TestData.REQUESTER_ID, UserRole.Requester),
            TestData.Clock);

    [Fact]
    public async Task HandleAsyncShouldMarkEveryUnreadNotificationOfCurrentUser()
    {
        Notification[] unread = [NewNotification(), NewNotification()];
        _notificationRepository
            .GetUnreadInAppForRecipientAsync(TestData.REQUESTER_ID, Arg.Any<CancellationToken>())
            .Returns(unread);
        MarkAllNotificationsAsReadCommand command = new();

        Result<Unit> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        unread.ShouldAllBe(notification => notification.ReadOnUtc == TestData.UTC_NOW);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsyncShouldNotSaveWhenNothingIsUnread()
    {
        _notificationRepository
            .GetUnreadInAppForRecipientAsync(TestData.REQUESTER_ID, Arg.Any<CancellationToken>())
            .Returns([]);
        MarkAllNotificationsAsReadCommand command = new();

        Result<Unit> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    private static Notification NewNotification()
        => Notification.Create(
            TestData.REQUESTER_ID,
            null,
            NotificationChannel.InApp,
            NotificationType.ServiceRequestStatusChanged,
            SUBJECT,
            MESSAGE,
            TestData.UTC_NOW);
}
