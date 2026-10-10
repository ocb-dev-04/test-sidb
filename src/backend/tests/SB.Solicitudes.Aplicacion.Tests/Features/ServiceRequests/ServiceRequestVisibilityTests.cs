using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Tests.Features.ServiceRequests;

public sealed class ServiceRequestVisibilityTests
{
    public static TheoryData<UserRole, Guid, Guid?, bool> Scenarios => new()
    {
        { UserRole.Administrator, TestData.ADMINISTRATOR_ID, TestData.OTHER_ANALYST_ID, true },
        { UserRole.Analyst, TestData.ANALYST_ID, null, true },
        { UserRole.Analyst, TestData.ANALYST_ID, TestData.ANALYST_ID, true },
        { UserRole.Analyst, TestData.ANALYST_ID, TestData.OTHER_ANALYST_ID, false },
        { UserRole.Requester, TestData.REQUESTER_ID, TestData.ANALYST_ID, true },
        { UserRole.Requester, TestData.OTHER_ANALYST_ID, null, false },
    };

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void CanViewAndQueryExpressionShouldAgree(UserRole role, Guid userId, Guid? assigneeId, bool expected)
    {
        ServiceRequest serviceRequest = TestData.NewServiceRequest();
        if (assigneeId is { } assignee)
        {
            serviceRequest.Assign(assignee, TestData.ADMINISTRATOR_ID, TestData.UTC_NOW);
        }

        ServiceRequestVisibility visibility = ServiceRequestVisibility.For(userId, role);

        visibility.CanView(serviceRequest).ShouldBe(expected);
        visibility.ToExpression().Compile()(serviceRequest).ShouldBe(expected);
    }
}
