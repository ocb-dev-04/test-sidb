using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Dominio.Tests.ServiceRequests;

public sealed class StatusTransitionsTests
{
    [Theory]
    [InlineData(ServiceRequestStatus.Registered, ServiceRequestStatus.InAnalysis)]
    [InlineData(ServiceRequestStatus.InAnalysis, ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.WaitingForRequester)]
    [InlineData(ServiceRequestStatus.WaitingForRequester, ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Resolved)]
    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.InAnalysis)]
    public void IsAllowedShouldBeTrueForWorkflowTransitions(ServiceRequestStatus from, ServiceRequestStatus to)
        => StatusTransitions.IsAllowed(from, to).ShouldBeTrue();

    [Theory]
    [InlineData(ServiceRequestStatus.Registered, ServiceRequestStatus.Resolved)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.Registered)]
    public void IsAllowedShouldBeFalseForTransitionsOutsideTheWorkflow(ServiceRequestStatus from, ServiceRequestStatus to)
        => StatusTransitions.IsAllowed(from, to).ShouldBeFalse();

    [Fact]
    public void EveryStatusShouldHaveAtLeastOneOutgoingTransition()
    {
        foreach (ServiceRequestStatus status in Enum.GetValues<ServiceRequestStatus>())
        {
            StatusTransitions.AllowedFrom(status).ShouldNotBeEmpty();
        }
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.WaitingForRequester, true)]
    public void IsOpenShouldExcludeResolvedAndClosed(ServiceRequestStatus status, bool expected)
        => StatusTransitions.IsOpen(status).ShouldBe(expected);
}
