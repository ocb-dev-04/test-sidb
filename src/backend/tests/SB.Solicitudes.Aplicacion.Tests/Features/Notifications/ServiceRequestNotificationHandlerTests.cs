using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.ServiceRequests.Events;

namespace SB.Solicitudes.Aplicacion.Tests.Features.Notifications;

public sealed class ServiceRequestNotificationHandlerTests
{
    private readonly IServiceRequestRepository _serviceRequestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly List<Notification> _queued = [];
    private readonly ServiceRequest _serviceRequest = TestData.NewServiceRequest();
    private readonly ServiceRequestNotificationHandler _handler;

    public ServiceRequestNotificationHandlerTests()
    {
        _serviceRequest.Assign(TestData.ANALYST_ID, TestData.ADMINISTRATOR_ID, TestData.UTC_NOW);
        _serviceRequestRepository.GetByIdAsync(_serviceRequest.Id, Arg.Any<CancellationToken>()).Returns(_serviceRequest);
        _notificationRepository.AddRange(Arg.Do<IEnumerable<Notification>>(notifications => _queued.AddRange(notifications)));

        NotificationOptions options = new() { Channels = [NotificationChannel.InApp, NotificationChannel.Email] };
        _handler = new ServiceRequestNotificationHandler(
            _serviceRequestRepository,
            _notificationRepository,
            Options.Create(options),
            NullLogger<ServiceRequestNotificationHandler>.Instance);
    }

    [Fact]
    public async Task StatusChangeShouldNotifyEveryoneExceptActorOnEveryChannel()
    {
        ServiceRequestStatusChangedDomainEvent domainEvent = new(
            _serviceRequest.Id, ServiceRequestStatus.InAnalysis, ServiceRequestStatus.Closed, TestData.ANALYST_ID, TestData.UTC_NOW);

        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        _queued.Select(notification => notification.RecipientUserId).Distinct().ShouldBe([TestData.REQUESTER_ID]);
        _queued.Select(notification => notification.Channel).ShouldBe([NotificationChannel.InApp, NotificationChannel.Email], ignoreOrder: true);
        _queued.ShouldAllBe(notification => notification.Type == NotificationType.ServiceRequestClosed);
    }

    [Fact]
    public async Task InternalCommentShouldNeverNotifyRequester()
    {
        ServiceRequestCommentAddedDomainEvent domainEvent = new(
            _serviceRequest.Id, Guid.NewGuid(), TestData.ADMINISTRATOR_ID, CommentVisibility.Internal, TestData.UTC_NOW);

        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        _queued.ShouldNotBeEmpty();
        _queued.ShouldAllBe(notification => notification.RecipientUserId == TestData.ANALYST_ID);
    }

    [Fact]
    public async Task CreationShouldConfirmToRequester()
    {
        ServiceRequestCreatedDomainEvent domainEvent = new(_serviceRequest.Id, TestData.REQUESTER_ID, TestData.UTC_NOW);

        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        _queued.ShouldAllBe(notification => notification.RecipientUserId == TestData.REQUESTER_ID
            && notification.Subject.Contains(TestData.CODE));
    }
}
