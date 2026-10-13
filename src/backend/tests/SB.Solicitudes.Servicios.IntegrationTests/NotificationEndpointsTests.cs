using System.Net;
using System.Net.Http.Json;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Servicios.IntegrationTests.Infrastructure;

namespace SB.Solicitudes.Servicios.IntegrationTests;

[Collection(ApiTestGroup.NAME)]
[Trait("Category", "Integration")]
public sealed class NotificationEndpointsTests(ApiFactory factory)
{
    private const string UNREAD_COUNT_ROUTE = "/api/notificaciones/no-leidas/total";
    private const string MARK_ALL_AS_READ_ROUTE = "/api/notificaciones/leidas";
    private const string TITLE = "Configurar firma digital";
    private const string DESCRIPTION = "Necesito la firma digital en el correo.";
    private const int SUPPORT_TYPE_ID = 1;
    private const int FINANCE_AREA_ID = 3;

    [Fact]
    public async Task MarkAllAsReadShouldClearOnlyCurrentUserNotifications()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.SECOND_REQUESTER_EMAIL);
        HttpClient administrator = await factory.CreateAuthenticatedClientAsync(ApiFactory.ADMINISTRATOR_EMAIL);
        CreateServiceRequestCommand command = new(TITLE, DESCRIPTION, ServiceRequestPriority.Low, SUPPORT_TYPE_ID, FINANCE_AREA_ID, null);
        HttpResponseMessage created = await requester.PostAsJsonAsync("/api/solicitudes", command, ApiFactory.JSON_OPTIONS);
        created.EnsureSuccessStatusCode();
        int requesterUnreadBefore = await UnreadCountAsync(requester);
        int administratorUnreadBefore = await UnreadCountAsync(administrator);

        HttpResponseMessage response = await requester.PatchAsync(MARK_ALL_AS_READ_ROUTE, null);

        requesterUnreadBefore.ShouldBeGreaterThan(0);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await UnreadCountAsync(requester)).ShouldBe(0);
        (await UnreadCountAsync(administrator)).ShouldBe(administratorUnreadBefore);
    }

    private static async Task<int> UnreadCountAsync(HttpClient client)
        => await client.GetFromJsonAsync<int>(UNREAD_COUNT_ROUTE, ApiFactory.JSON_OPTIONS);
}
