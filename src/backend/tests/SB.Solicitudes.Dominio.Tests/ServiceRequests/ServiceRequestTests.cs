using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.ServiceRequests.Events;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Dominio.Tests.ServiceRequests;

public sealed class ServiceRequestTests
{
    private const string RESOLUTION_COMMENT = "Se restableció la contraseña de la VPN.";
    private const string NEW_TITLE = "Acceso a VPN corporativa";
    private const int HOURS_AFTER_CREATION = 200;

    private static readonly DateTime LATER_UTC = ServiceRequestBuilder.CREATED_ON_UTC.AddHours(1);

    [Theory]
    [InlineData(ServiceRequestPriority.Critical, PriorityServiceLevels.CRITICAL_RESOLUTION_HOURS)]
    [InlineData(ServiceRequestPriority.High, PriorityServiceLevels.HIGH_RESOLUTION_HOURS)]
    [InlineData(ServiceRequestPriority.Medium, PriorityServiceLevels.MEDIUM_RESOLUTION_HOURS)]
    [InlineData(ServiceRequestPriority.Low, PriorityServiceLevels.LOW_RESOLUTION_HOURS)]
    public void CreateShouldSetDueDateFromPriorityServiceLevel(ServiceRequestPriority priority, int expectedHours)
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered(priority);

        serviceRequest.DueDateUtc.ShouldBe(ServiceRequestBuilder.CREATED_ON_UTC.AddHours(expectedHours));
    }

    [Fact]
    public void CreateShouldStartRegisteredWithHistoryAndCreatedEvent()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered();

        serviceRequest.Status.ShouldBe(ServiceRequestStatus.Registered);
        StatusHistory history = serviceRequest.StatusHistory.ShouldHaveSingleItem();
        history.PreviousStatus.ShouldBeNull();
        history.NewStatus.ShouldBe(ServiceRequestStatus.Registered);
        history.ChangedById.ShouldBe(ServiceRequestBuilder.REQUESTER_ID);
        serviceRequest.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ServiceRequestCreatedDomainEvent>();
    }

    [Fact]
    public void ChangeStatusShouldRecordHistoryAndRaiseEventWhenTransitionIsAllowed()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.Registered);

        Result result = serviceRequest.ChangeStatus(
            ServiceRequestStatus.InAnalysis, ServiceRequestBuilder.ANALYST_ID, UserRole.Analyst, null, LATER_UTC);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.Status.ShouldBe(ServiceRequestStatus.InAnalysis);
        StatusHistory lastHistory = serviceRequest.StatusHistory.Last();
        lastHistory.PreviousStatus.ShouldBe(ServiceRequestStatus.Registered);
        lastHistory.ChangedById.ShouldBe(ServiceRequestBuilder.ANALYST_ID);
        lastHistory.ChangedOnUtc.ShouldBe(LATER_UTC);
        ServiceRequestStatusChangedDomainEvent domainEvent = serviceRequest.DomainEvents
            .ShouldHaveSingleItem()
            .ShouldBeOfType<ServiceRequestStatusChangedDomainEvent>();
        domainEvent.NewStatus.ShouldBe(ServiceRequestStatus.InAnalysis);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Registered, ServiceRequestStatus.Resolved)]
    [InlineData(ServiceRequestStatus.Registered, ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.Resolved)]
    public void ChangeStatusShouldFailWhenTransitionIsNotAllowed(ServiceRequestStatus from, ServiceRequestStatus to)
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(from);

        Result result = serviceRequest.ChangeStatus(to, ServiceRequestBuilder.ANALYST_ID, UserRole.Analyst, RESOLUTION_COMMENT, LATER_UTC);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(ServiceRequestErrors.InvalidTransition(from, to).Code);
        serviceRequest.Status.ShouldBe(from);
        serviceRequest.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ChangeStatusShouldFailWhenStatusIsTheSame()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.InAnalysis);

        Result result = serviceRequest.ChangeStatus(
            ServiceRequestStatus.InAnalysis, ServiceRequestBuilder.ANALYST_ID, UserRole.Analyst, null, LATER_UTC);

        result.Error.ShouldBe(ServiceRequestErrors.SAME_STATUS);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.InAnalysis, ServiceRequestStatus.Resolved)]
    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Registered, ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.InAnalysis)]
    public void ChangeStatusShouldRequireCommentWhenResolvingClosingOrReopening(ServiceRequestStatus from, ServiceRequestStatus to)
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(from);

        Result result = serviceRequest.ChangeStatus(to, ServiceRequestBuilder.ANALYST_ID, UserRole.Analyst, "   ", LATER_UTC);

        result.Error.ShouldBe(ServiceRequestErrors.COMMENT_REQUIRED);
        serviceRequest.Status.ShouldBe(from);
    }

    [Fact]
    public void ChangeStatusShouldStoreResolutionCommentWhenClosing()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.InAnalysis);

        Result result = serviceRequest.ChangeStatus(
            ServiceRequestStatus.Closed, ServiceRequestBuilder.ANALYST_ID, UserRole.Analyst, RESOLUTION_COMMENT, LATER_UTC);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.ClosedOnUtc.ShouldBe(LATER_UTC);
        serviceRequest.ResolutionComment.ShouldBe(RESOLUTION_COMMENT);
        serviceRequest.StatusHistory.Last().Comment.ShouldBe(RESOLUTION_COMMENT);
    }

    [Fact]
    public void ChangeStatusShouldRejectReopeningWhenActorIsRequester()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.Closed);

        Result result = serviceRequest.ChangeStatus(
            ServiceRequestStatus.InAnalysis, ServiceRequestBuilder.REQUESTER_ID, UserRole.Requester, RESOLUTION_COMMENT, LATER_UTC);

        result.Error.ShouldBe(ServiceRequestErrors.REOPEN_NOT_ALLOWED);
        serviceRequest.Status.ShouldBe(ServiceRequestStatus.Closed);
    }

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.Analyst)]
    public void ChangeStatusShouldReopenAndClearClosureWhenActorManagesRequests(UserRole role)
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.Closed);

        Result result = serviceRequest.ChangeStatus(
            ServiceRequestStatus.InAnalysis, ServiceRequestBuilder.ANALYST_ID, role, RESOLUTION_COMMENT, LATER_UTC);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.Status.ShouldBe(ServiceRequestStatus.InAnalysis);
        serviceRequest.ClosedOnUtc.ShouldBeNull();
        serviceRequest.ResolvedOnUtc.ShouldBeNull();
        serviceRequest.ResolutionComment.ShouldBeNull();
    }

    [Fact]
    public void AssignShouldRaiseEventWithPreviousAssignee()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered();
        serviceRequest.ClearDomainEvents();
        Guid secondAnalystId = Guid.NewGuid();
        serviceRequest.Assign(ServiceRequestBuilder.ANALYST_ID, ServiceRequestBuilder.ANALYST_ID, LATER_UTC);

        Result result = serviceRequest.Assign(secondAnalystId, ServiceRequestBuilder.ANALYST_ID, LATER_UTC);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.AssigneeId.ShouldBe(secondAnalystId);
        ServiceRequestAssignedDomainEvent domainEvent = serviceRequest.DomainEvents.Last().ShouldBeOfType<ServiceRequestAssignedDomainEvent>();
        domainEvent.PreviousAssigneeId.ShouldBe(ServiceRequestBuilder.ANALYST_ID);
    }

    [Fact]
    public void AssignShouldFailWhenAlreadyAssignedToSameUser()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered();
        serviceRequest.Assign(ServiceRequestBuilder.ANALYST_ID, ServiceRequestBuilder.ANALYST_ID, LATER_UTC);

        Result result = serviceRequest.Assign(ServiceRequestBuilder.ANALYST_ID, ServiceRequestBuilder.ANALYST_ID, LATER_UTC);

        result.Error.ShouldBe(ServiceRequestErrors.ALREADY_ASSIGNED);
    }

    [Fact]
    public void ClosedRequestShouldRejectAssignmentAndEdition()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.Closed);

        Result assignResult = serviceRequest.Assign(Guid.NewGuid(), ServiceRequestBuilder.ANALYST_ID, LATER_UTC);
        Result updateResult = serviceRequest.Update(NEW_TITLE, null, null, null, null, null, LATER_UTC);

        assignResult.Error.ShouldBe(ServiceRequestErrors.CLOSED_CANNOT_BE_MODIFIED);
        updateResult.Error.ShouldBe(ServiceRequestErrors.CLOSED_CANNOT_BE_MODIFIED);
    }

    [Fact]
    public void UpdateShouldRecalculateDueDateWhenPriorityChanges()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered(ServiceRequestPriority.Low);

        Result result = serviceRequest.Update(NEW_TITLE, null, ServiceRequestPriority.Critical, null, null, null, LATER_UTC);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.Title.ShouldBe(NEW_TITLE);
        serviceRequest.Description.ShouldBe(ServiceRequestBuilder.DESCRIPTION);
        serviceRequest.DueDateUtc.ShouldBe(ServiceRequestBuilder.CREATED_ON_UTC.AddHours(PriorityServiceLevels.CRITICAL_RESOLUTION_HOURS));
    }

    [Fact]
    public void IsOverdueShouldBeTrueOnlyWhileOpenAndPastDueDate()
    {
        ServiceRequest openRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.InProgress);
        ServiceRequest closedRequest = ServiceRequestBuilder.InStatus(ServiceRequestStatus.Closed);
        DateTime afterDueDate = ServiceRequestBuilder.CREATED_ON_UTC.AddHours(HOURS_AFTER_CREATION);

        openRequest.IsOverdue(afterDueDate).ShouldBeTrue();
        openRequest.IsOverdue(LATER_UTC).ShouldBeFalse();
        closedRequest.IsOverdue(afterDueDate).ShouldBeFalse();
    }

    [Fact]
    public void AddCommentShouldStoreCommentAndRaiseEvent()
    {
        ServiceRequest serviceRequest = ServiceRequestBuilder.Registered();
        serviceRequest.ClearDomainEvents();

        Comment comment = serviceRequest.AddComment(ServiceRequestBuilder.ANALYST_ID, RESOLUTION_COMMENT, CommentVisibility.Internal, LATER_UTC);

        serviceRequest.Comments.ShouldHaveSingleItem().ShouldBe(comment);
        comment.Visibility.ShouldBe(CommentVisibility.Internal);
        serviceRequest.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ServiceRequestCommentAddedDomainEvent>();
    }
}
