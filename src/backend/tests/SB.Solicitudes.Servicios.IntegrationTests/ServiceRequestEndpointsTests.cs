using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.Authentication;
using SB.Solicitudes.Aplicacion.Features.Dashboard;
using SB.Solicitudes.Aplicacion.Features.Notifications;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Servicios.Controllers;
using SB.Solicitudes.Servicios.IntegrationTests.Infrastructure;

namespace SB.Solicitudes.Servicios.IntegrationTests;

[Collection(ApiTestGroup.NAME)]
[Trait("Category", "Integration")]
public sealed class ServiceRequestEndpointsTests(ApiFactory factory)
{
    private const string SOLICITUDES_ROUTE = "/api/solicitudes";
    private const string ERROR_CODE = "code";
    private const string RESOLUTION_COMMENT = "Se instaló el software solicitado.";
    private const string NEW_TITLE = "Instalar Power BI Desktop";
    private const string NEW_DESCRIPTION = "Necesito Power BI para los tableros del área.";
    private const int SUPPORT_TYPE_ID = 1;
    private const int FINANCE_AREA_ID = 3;
    private const int OVERSIZED_PAGE_SIZE = 1000;

    private static readonly Guid SEEDED_REQUEST_ASSIGNED_TO_SECOND_ANALYST = new(2, 0, 0, [0, 0, 0, 0, 0, 0, 0, 2]);
    private static readonly Guid SEEDED_CLOSED_REQUEST_OF_FIRST_REQUESTER = new(1, 0, 0, [0, 0, 0, 0, 0, 0, 0, 2]);

    [Fact]
    public async Task LoginShouldRejectWrongPasswordWithoutRevealingWhetherUserExists()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand(ApiFactory.ADMINISTRATOR_EMAIL, "Wrong123*"));
        HttpResponseMessage unknownUser = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand("nadie@solicitudes.local", "Wrong123*"));

        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ErrorCodeAsync(wrongPassword)).ShouldBe(await ErrorCodeAsync(unknownUser));
    }

    [Fact]
    public async Task EndpointsShouldRequireAuthentication()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync(SOLICITUDES_ROUTE);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequesterShouldCreateAndOnlyListOwnRequests()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.SECOND_REQUESTER_EMAIL);
        UserProfile profile = await GetProfileAsync(requester);

        HttpResponseMessage created = await requester.PostAsJsonAsync(
            SOLICITUDES_ROUTE,
            new CreateServiceRequestCommand(NEW_TITLE, NEW_DESCRIPTION, ServiceRequestPriority.Medium, SUPPORT_TYPE_ID, FINANCE_AREA_ID, null),
            ApiFactory.JSON_OPTIONS);
        PagedResult<ServiceRequestSummaryResponse> page = await GetJsonAsync<PagedResult<ServiceRequestSummaryResponse>>(requester, SOLICITUDES_ROUTE + "?pageSize=100");

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        ServiceRequestCreatedResponse body = (await created.Content.ReadFromJsonAsync<ServiceRequestCreatedResponse>(ApiFactory.JSON_OPTIONS))!;
        body.Code.ShouldStartWith($"{ServiceRequestCode.PREFIX}-{DateTime.UtcNow.Year}-");
        page.Items.ShouldContain(item => item.Id == body.Id);
        page.Items.ShouldAllBe(item => item.RequesterId == profile.Id);
    }

    [Fact]
    public async Task RequesterShouldNotChangeStatus()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);

        HttpResponseMessage response = await requester.PatchAsJsonAsync(
            $"{SOLICITUDES_ROUTE}/{SEEDED_CLOSED_REQUEST_OF_FIRST_REQUESTER}/estado",
            new ChangeStatusRequest(ServiceRequestStatus.InAnalysis, RESOLUTION_COMMENT),
            ApiFactory.JSON_OPTIONS);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClosingShouldRequireCommentAndNotifyRequester()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);
        HttpClient analyst = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_ANALYST_EMAIL);
        HttpResponseMessage created = await requester.PostAsJsonAsync(
            SOLICITUDES_ROUTE,
            new CreateServiceRequestCommand(NEW_TITLE, NEW_DESCRIPTION, ServiceRequestPriority.High, SUPPORT_TYPE_ID, FINANCE_AREA_ID, null),
            ApiFactory.JSON_OPTIONS);
        Guid id = (await created.Content.ReadFromJsonAsync<ServiceRequestCreatedResponse>(ApiFactory.JSON_OPTIONS))!.Id;
        string statusRoute = $"{SOLICITUDES_ROUTE}/{id}/estado";

        HttpResponseMessage withoutComment = await analyst.PatchAsJsonAsync(statusRoute, new ChangeStatusRequest(ServiceRequestStatus.Closed, null), ApiFactory.JSON_OPTIONS);
        HttpResponseMessage withComment = await analyst.PatchAsJsonAsync(statusRoute, new ChangeStatusRequest(ServiceRequestStatus.Closed, RESOLUTION_COMMENT), ApiFactory.JSON_OPTIONS);
        ServiceRequestDetailResponse detail = await GetJsonAsync<ServiceRequestDetailResponse>(requester, $"{SOLICITUDES_ROUTE}/{id}");
        PagedResult<NotificationResponse> notifications = await GetJsonAsync<PagedResult<NotificationResponse>>(requester, "/api/notificaciones?pageSize=50");

        withoutComment.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorCodeAsync(withoutComment)).ShouldBe(ServiceRequestErrors.COMMENT_REQUIRED.Code);
        withComment.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        detail.Status.ShouldBe(ServiceRequestStatus.Closed);
        detail.ResolutionComment.ShouldBe(RESOLUTION_COMMENT);
        detail.StatusHistory.Select(history => history.NewStatus).ShouldBe([ServiceRequestStatus.Registered, ServiceRequestStatus.Closed]);
        notifications.Items.ShouldContain(notification => notification.ServiceRequestId == id && notification.Type == NotificationType.ServiceRequestClosed);
    }

    [Fact]
    public async Task AnalystShouldNotSeeRequestsAssignedToAnotherAnalyst()
    {
        HttpClient analyst = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_ANALYST_EMAIL);

        HttpResponseMessage response = await analyst.GetAsync($"{SOLICITUDES_ROUTE}/{SEEDED_REQUEST_ASSIGNED_TO_SECOND_ANALYST}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequesterShouldNotSeeInternalComments()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);
        HttpClient administrator = await factory.CreateAuthenticatedClientAsync(ApiFactory.ADMINISTRATOR_EMAIL);
        string route = $"{SOLICITUDES_ROUTE}/{SEEDED_CLOSED_REQUEST_OF_FIRST_REQUESTER}";

        ServiceRequestDetailResponse requesterView = await GetJsonAsync<ServiceRequestDetailResponse>(requester, route);
        ServiceRequestDetailResponse administratorView = await GetJsonAsync<ServiceRequestDetailResponse>(administrator, route);

        requesterView.Comments.ShouldAllBe(comment => comment.Visibility == CommentVisibility.Public);
        administratorView.Comments.ShouldContain(comment => comment.Visibility == CommentVisibility.Internal);
        requesterView.AllowedTransitions.ShouldBeEmpty();
        administratorView.AllowedTransitions.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ListShouldValidatePageSize()
    {
        HttpClient administrator = await factory.CreateAuthenticatedClientAsync(ApiFactory.ADMINISTRATOR_EMAIL);

        HttpResponseMessage response = await administrator.GetAsync($"{SOLICITUDES_ROUTE}?pageSize={OVERSIZED_PAGE_SIZE}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.ShouldContainKey("pageSize");
    }

    [Fact]
    public async Task DashboardShouldBeConsistentWithItsBreakdowns()
    {
        HttpClient administrator = await factory.CreateAuthenticatedClientAsync(ApiFactory.ADMINISTRATOR_EMAIL);

        DashboardSummaryResponse summary = await GetJsonAsync<DashboardSummaryResponse>(administrator, "/api/dashboard/resumen");

        summary.TotalCount.ShouldBe(summary.ByStatus.Sum(item => item.Count));
        summary.TotalCount.ShouldBe(summary.ByPriority.Sum(item => item.Count));
        summary.LatestRequests.ShouldNotBeEmpty();
        summary.LatestRequests.Count.ShouldBeLessThanOrEqualTo(GetDashboardSummaryQuery.LATEST_REQUESTS_COUNT);
    }

    private static async Task<T> GetJsonAsync<T>(HttpClient client, string route)
    {
        HttpResponseMessage response = await client.GetAsync(route);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ApiFactory.JSON_OPTIONS))!;
    }

    private static async Task<UserProfile> GetProfileAsync(HttpClient client)
        => await GetJsonAsync<UserProfile>(client, "/api/auth/me");

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.TryGetProperty(ERROR_CODE, out JsonElement code) ? code.GetString() : null;
    }

    private sealed record UserProfile(Guid Id, string Email, UserRole Role);
}
