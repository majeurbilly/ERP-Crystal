using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Crystal.Core;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.IntegrationTests.Items;

public sealed class ItemIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public ItemIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetInventory_Returns200()
    {
        // Préparation : authentification avec un token Employee valide.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Exécution
        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ReturnsItemFields()
    {
        // Préparation : authentification avec un token Employee valide.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new Item
        {
            Name = "Clean Code",
            Description = "Livre de programmation",
            Price = 32.50m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ItemResponseDto>? body = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();

        Assert.NotNull(body);
        ItemResponseDto dto = Assert.Single(body);

        Assert.Equal("Clean Code", dto.Name);
        Assert.Equal("Livre de programmation", dto.Description);
        Assert.Equal(32.50m, dto.Price);
        Assert.Equal(5, dto.AlertQuantity);
    }

    [Fact]
    public async Task GetInventory_DoesNotReturnInactiveItems()
    {
        // Préparation : authentification avec un token Employee valide.
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        context.Items.AddRange(
            new Item
            {
                Name = "Item actif",
                Description = "Visible",
                Price = 10m,
                AlertQuantity = 2,
                LastUpdate = DateTime.UtcNow,
                IsActive = true
            },
            new Item
            {
                Name = "Item inactif",
                Description = "Invisible",
                Price = 20m,
                AlertQuantity = 2,
                LastUpdate = DateTime.UtcNow,
                IsActive = false
            });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ItemResponseDto>? body = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();

        Assert.NotNull(body);
        Assert.Contains(body, i => i.Name == "Item actif");
        Assert.DoesNotContain(body, i => i.Name == "Item inactif");
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
}