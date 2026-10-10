using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SB.Solicitudes.Aplicacion.Abstractions.Pagination;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.GovernmentEntities;
using SB.Solicitudes.Servicios.Controllers;
using SB.Solicitudes.Servicios.IntegrationTests.Infrastructure;

namespace SB.Solicitudes.Servicios.IntegrationTests;

[Collection(ApiTestGroup.NAME)]
[Trait("Category", "Integration")]
public sealed class GovernmentEntityEndpointsTests(ApiFactory factory)
{
    private const string ROUTE = "/api/entidades-gubernamentales";
    private const int EXCEL_ENTITY_COUNT = 181;
    private const string CATEGORY = "Organismo Descentralizado Funcionalmente";
    private const string STATE_BRANCH = "Poder Ejecutivo";
    private const string SECTOR = "Hacienda y Economía";

    [Theory]
    [InlineData("TextFile")]
    [InlineData("Sqlite")]
    [InlineData("Excel")]
    public async Task EverySourceShouldServeTheSeededCatalog(string source)
    {
        HttpClient client = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);

        PagedResult<GovernmentEntityResponse>? page = await client.GetFromJsonAsync<PagedResult<GovernmentEntityResponse>>(
            $"{ROUTE}?source={source}&search=universidad", ApiFactory.JSON_OPTIONS);
        PagedResult<GovernmentEntityResponse>? excel = await client.GetFromJsonAsync<PagedResult<GovernmentEntityResponse>>(
            $"{ROUTE}?source=Excel", ApiFactory.JSON_OPTIONS);

        page!.Items.ShouldNotBeEmpty();
        page.Items.ShouldAllBe(entity => entity.Name.Contains("Universidad", StringComparison.OrdinalIgnoreCase));
        excel!.TotalCount.ShouldBe(EXCEL_ENTITY_COUNT);
    }

    [Fact]
    public async Task AdministratorShouldCrudOnTextFileButNotOnExcel()
    {
        HttpClient administrator = await factory.CreateAuthenticatedClientAsync(ApiFactory.ADMINISTRATOR_EMAIL);
        GovernmentEntityRequest request = new($"Entidad de prueba {Guid.NewGuid():N}", CATEGORY, STATE_BRANCH, SECTOR);

        HttpResponseMessage excelCreate = await administrator.PostAsJsonAsync($"{ROUTE}?source=Excel", request, ApiFactory.JSON_OPTIONS);
        HttpResponseMessage created = await administrator.PostAsJsonAsync($"{ROUTE}?source=TextFile", request, ApiFactory.JSON_OPTIONS);
        GovernmentEntityResponse entity = (await created.Content.ReadFromJsonAsync<GovernmentEntityResponse>(ApiFactory.JSON_OPTIONS))!;
        HttpResponseMessage duplicate = await administrator.PostAsJsonAsync($"{ROUTE}?source=TextFile", request, ApiFactory.JSON_OPTIONS);
        HttpResponseMessage updated = await administrator.PutAsJsonAsync($"{ROUTE}/{entity.Id}?source=TextFile", request with { Sector = STATE_BRANCH }, ApiFactory.JSON_OPTIONS);
        HttpResponseMessage deleted = await administrator.DeleteAsync($"{ROUTE}/{entity.Id}?source=TextFile");
        HttpResponseMessage afterDelete = await administrator.GetAsync($"{ROUTE}/{entity.Id}?source=TextFile");

        excelCreate.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorCodeAsync(excelCreate)).ShouldBe(GovernmentEntityErrors.SOURCE_READ_ONLY.Code);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        updated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        afterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequesterShouldNotModifyTheCatalog()
    {
        HttpClient requester = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);

        HttpResponseMessage response = await requester.PostAsJsonAsync(
            ROUTE, new GovernmentEntityRequest("No permitido", CATEGORY, STATE_BRANCH, SECTOR), ApiFactory.JSON_OPTIONS);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SourcesEndpointShouldFlagExcelAsReadOnly()
    {
        HttpClient client = await factory.CreateAuthenticatedClientAsync(ApiFactory.FIRST_REQUESTER_EMAIL);

        List<GovernmentEntityDataSourceResponse>? sources = await client.GetFromJsonAsync<List<GovernmentEntityDataSourceResponse>>(
            $"{ROUTE}/fuentes", ApiFactory.JSON_OPTIONS);

        sources.ShouldNotBeNull();
        sources.Single(source => source.Source == GovernmentEntityDataSource.Excel).IsReadOnly.ShouldBeTrue();
        sources.Single(source => source.IsDefault).Source.ShouldBe(GovernmentEntityDataSource.TextFile);
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.TryGetProperty("code", out JsonElement code) ? code.GetString() : null;
    }
}
