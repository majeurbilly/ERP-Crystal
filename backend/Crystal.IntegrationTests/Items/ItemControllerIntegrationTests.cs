using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.IntegrationTests.Items;

public sealed class ItemControllerIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public ItemControllerIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetItems_WithValidToken_Returns200Ok()
    {
        // Arrange : authentification avec le role Employee.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ItemResponse>? body = await response.Content.ReadFromJsonAsync<List<ItemResponse>>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetItems_WithoutAuth_Returns401Unauthorized()
    {
        // Arrange : suppression explicite du token.
        m_client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetItems_ReturnsOnlyActiveItems()
    {
        // Arrange : preparation des donnees avec un item actif et un inactif.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item activeItem = new Item
        {
            Name = "Active integration item",
            Description = "Should be returned",
            Price = 18m,
            AlertQuantity = 4,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item inactiveItem = new Item
        {
            Name = "Inactive integration item",
            Description = "Should not be returned",
            Price = 21m,
            AlertQuantity = 4,
            LastUpdate = DateTime.UtcNow,
            IsActive = false
        };

        context.Items.Add(activeItem);
        context.Items.Add(inactiveItem);
        await context.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ItemResponse>? body = await response.Content.ReadFromJsonAsync<List<ItemResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, i => i.Name == "Active integration item");
        Assert.DoesNotContain(body, i => i.Name == "Inactive integration item");
    }

    [Fact]
    public async Task GetItemById_WithValidId_Returns200Ok()
    {
        // Arrange : creation d'un item actif puis authentification Employee.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Item item = new Item
        {
            Name = "Valid id integration item",
            Description = "Details item",
            Price = 11m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync($"/api/items/{item.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ItemResponse? body = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(body);
        Assert.Equal(item.Id, body.Id);
    }

    [Fact]
    public async Task GetItemById_WithInvalidId_Returns404NotFound()
    {
        // Arrange : authentification Employee.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/items/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItemById_WithoutAuth_Returns401Unauthorized()
    {
        // Arrange : suppression explicite du token.
        m_client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/items/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_WithAdminRole_Returns201Created()
    {
        // Arrange : authentification Admin et payload valide.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        CreateItemRequest request = new CreateItemRequest
        {
            Name = "Integration created item",
            Description = "Created by admin integration test",
            Price = 35m,
            AlertQuantity = 4,
            InitialQuantity = 0
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateItem_WithEmployeeRole_Returns403Forbidden()
    {
        // Arrange : authentification Employee.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        CreateItemRequest request = new CreateItemRequest
        {
            Name = "Forbidden item",
            Description = "Employee cannot create item",
            Price = 10m,
            AlertQuantity = 1,
            InitialQuantity = 0
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_WithInvalidData_Returns400BadRequest()
    {
        // Arrange : authentification Admin avec payload invalide.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        CreateItemRequest request = new CreateItemRequest
        {
            Name = string.Empty,
            Description = "Invalid payload",
            Price = -1m,
            AlertQuantity = 0,
            InitialQuantity = 0
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithAdminRole_Returns200Ok()
    {
        // Arrange : recupere un item existant puis authentification Admin.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Item item = new Item
        {
            Name = "Update source item",
            Description = "Before update",
            Price = 13m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        UpdateItemRequest request = new UpdateItemRequest
        {
            Name = "Updated integration name",
            Description = "After update",
            Price = 19m,
            AlertQuantity = 5,
            BookId = null
        };

        // Act
        HttpResponseMessage response = await m_client.PutAsJsonAsync($"/api/items/{item.Id}", request);
        ItemResponse? body = await response.Content.ReadFromJsonAsync<ItemResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Updated integration name", body.Name);
    }

    [Fact]
    public async Task UpdateItem_WithEmployeeRole_Returns403Forbidden()
    {
        // Arrange : authentification Employee.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        UpdateItemRequest request = new UpdateItemRequest
        {
            Name = "Forbidden update",
            Description = "Employee cannot update",
            Price = 12m,
            AlertQuantity = 1,
            BookId = null
        };

        // Act
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/items/1", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithInvalidId_Returns404NotFound()
    {
        // Arrange : authentification Admin.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        UpdateItemRequest request = new UpdateItemRequest
        {
            Name = "Unknown update",
            Description = "Unknown item",
            Price = 15m,
            AlertQuantity = 2,
            BookId = null
        };

        // Act
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/items/999999", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_WithAdminRole_Returns204NoContent()
    {
        // Arrange : creation d'un item actif puis authentification Admin.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Item item = new Item
        {
            Name = "Delete integration target",
            Description = "Item to soft delete",
            Price = 14m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        // Act
        HttpResponseMessage response = await m_client.DeleteAsync($"/api/items/{item.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using IServiceScope assertScope = m_factory.Services.CreateScope();
        CrystalDbContext assertContext = assertScope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Item? deletedItem = await assertContext.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
        Assert.NotNull(deletedItem);
        Assert.False(deletedItem.IsActive);
    }

    [Fact]
    public async Task DeleteItem_WithEmployeeRole_Returns403Forbidden()
    {
        // Arrange : authentification Employee.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.DeleteAsync("/api/items/1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
}
