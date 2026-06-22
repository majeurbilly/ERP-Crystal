using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crystal.IntegrationTests.Locations;

public sealed class LocationIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public LocationIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200_WhenGerantAuthenticated()
    {
        await AuthenticateAsGerantAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/locations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Returns200_WhenEmployeeAuthenticated()
    {
        await AuthenticateAsEmployeeAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/locations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns200_WhenEmployeeAuthenticated()
    {
        await AuthenticateAsEmployeeAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Location location = new()
        {
            Title = "Employee Branch",
            Address = "99 Employee Street",
            Description = "Accessible en lecture"
        };

        context.Locations.Add(location);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync($"/api/locations/{location.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        LocationResponseDto? body = await response.Content.ReadFromJsonAsync<LocationResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(location.Id, body.Id);
        Assert.Equal("Employee Branch", body.Title);
        Assert.Equal("99 Employee Street", body.Address);
    }

    [Fact]
    public async Task GetDropdown_ReturnsLightweightOptions_WhenEmployeeAuthenticated()
    {
        await AuthenticateAsEmployeeAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Location locationA = new()
        {
            Title = "Alpha Branch",
            Address = "1 rue A",
            Description = "Description longue A"
        };

        Location locationB = new()
        {
            Title = "Beta Branch",
            Address = "2 rue B",
            Description = "Description longue B"
        };

        context.Locations.AddRange(locationA, locationB);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/locations/dropdown");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync();

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        List<LocationOptionResponseDto>? body =
            JsonSerializer.Deserialize<List<LocationOptionResponseDto>>(json, jsonOptions);

        Assert.NotNull(body);
        Assert.Equal(2, body.Count);

        LocationOptionResponseDto first = body[0];
        LocationOptionResponseDto second = body[1];

        Assert.Equal("Alpha Branch", first.Title);
        Assert.True(first.Id > 0);
        Assert.Equal("Beta Branch", second.Title);
        Assert.True(second.Id > 0);

        Assert.DoesNotContain("address", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("description", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_Returns403_WhenGerantAuthenticated()
    {
        await AuthenticateAsGerantAsync();
        CreateLocationRequestDto request = new()
        {
            Title = "Test Branch",
            Address = "123 rue Principale",
            Description = "Description test"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/locations", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201_WhenAdminAuthenticated()
    {
        await AuthenticateAsAdminAsync();
        CreateLocationRequestDto request = new()
        {
            Title = "North Branch",
            Address = "456 avenue Centrale",
            Description = "Nouvelle succursale"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/locations", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        LocationResponseDto? body = await response.Content.ReadFromJsonAsync<LocationResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal("North Branch", body.Title);
    }

    [Fact]
    public async Task Create_Returns409_WhenTitleAlreadyExists()
    {
        await AuthenticateAsAdminAsync();
        CreateLocationRequestDto request = new()
        {
            Title = "Unique Branch",
            Address = "1 rue A",
            Description = "A"
        };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/locations", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        HttpResponseMessage duplicateResponse = await m_client.PostAsJsonAsync("/api/locations", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("A location with this title already exists.", message);
    }

    [Fact]
    public async Task Delete_Returns409_WhenLocationHasInventory()
    {
        await AuthenticateAsAdminAsync();

        using (IServiceScope scope = m_factory.Services.CreateScope())
        {
            CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

            Crystal.Core.Entities.Location location = new()
            {
                Title = "Branch With Stock",
                Address = "99 rue Inventaire",
                Description = "Stock présent"
            };

            Item item = new()
            {
                Name = "Article bloquant suppression",
                Price = 10,
                AlertQuantity = 1,
                LastUpdate = DateTime.UtcNow,
                IsActive = true
            };

            context.Locations.Add(location);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            context.InventoryLines.Add(new InventoryLine
            {
                ItemId = item.Id,
                LocationId = location.Id,
                Quantity = 3
            });
            await context.SaveChangesAsync();

            HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/locations/{location.Id}");
            Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);

            JsonDocument document = JsonDocument.Parse(await deleteResponse.Content.ReadAsStringAsync());
            string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
            Assert.Equal(
                ErrorMessages.Location.HasInventoryCannotDelete,
                message);
        }
    }

    [Fact]
    public async Task Delete_Returns204_WhenLocationHasNoInventory()
    {
        await AuthenticateAsAdminAsync();

        CreateLocationRequestDto request = new()
        {
            Title = "Empty Branch",
            Address = "2 rue B",
            Description = "Sans inventaire"
        };

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/locations", request);
        createResponse.EnsureSuccessStatusCode();

        LocationResponseDto? created = await createResponse.Content.ReadFromJsonAsync<LocationResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns403_WhenGerantAuthenticated()
    {
        await AuthenticateAsGerantAsync();
        HttpResponseMessage response = await m_client.DeleteAsync("/api/locations/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin@crystal.local");
    }

    private async Task AuthenticateAsGerantAsync()
    {
        await AuthenticateAsync("gerant@crystal.local");
    }

    private async Task AuthenticateAsEmployeeAsync()
    {
        await AuthenticateAsync("employee@crystal.local");
    }

    private async Task AuthenticateAsync(string p_email)
    {
        LoginRequest request = new()
        {
            Email = p_email,
            Password = "ValidPass1!a"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
