using Crystal.Core;
using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Crystal.Infrastructure.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Inventory;

public sealed class InventoryIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public InventoryIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task UpdateQuantity_WhenInventoryLineDoesNotExist_CreatesLine()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Clean Code",
            Description = "Livre",
            Price = 32.50m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Quebec City Branch",
            Address = "123 Rue Test",
            Description = "Magasin principal"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 10
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(10, line.Quantity);
    }

    [Fact]
    public async Task UpdateQuantity_WhenInventoryLineExists_ReplacesQuantity()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Refactoring",
            Description = "Livre",
            Price = 40m,
            AlertQuantity = 3,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Montreal Branch",
            Address = "456 Rue Test",
            Description = "Magasin secondaire"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 4
        });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 15
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(15, line.Quantity);
    }

    [Fact]
    public async Task AddQuantity_WhenInventoryLineExists_AddsToExistingQuantity()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Pragmatic Programmer",
            Description = "Livre",
            Price = 35m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Levis Branch",
            Address = "789 Rue Test",
            Description = "Magasin"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 7
        });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 5
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/inventory/quantity/add", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(12, line.Quantity);
    }

    [Fact]
    public async Task AddQuantity_WhenItemExistsInMultipleLocations_OnlyUpdatesTargetLocation()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Clean Architecture",
            Description = "Livre",
            Price = 42m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location quebec = new()
        {
            Title = "Quebec City Branch",
            Address = "123 Rue Test",
            Description = "Magasin Québec"
        };

        Location montreal = new()
        {
            Title = "Montreal Branch",
            Address = "456 Rue Test",
            Description = "Magasin Montréal"
        };

        context.Items.Add(item);
        context.Locations.AddRange(quebec, montreal);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = quebec.Id,
                Quantity = 7
            },
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = montreal.Id,
                Quantity = 20
            });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = quebec.Id,
            Quantity = 5
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/inventory/quantity/add", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? quebecLine = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == quebec.Id);

        InventoryLine? montrealLine = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == montreal.Id);

        Assert.NotNull(quebecLine);
        Assert.NotNull(montrealLine);

        Assert.Equal(12, quebecLine.Quantity);
        Assert.Equal(20, montrealLine.Quantity);
    }

    [Fact]
    public async Task UpdateQuantity_WhenItemExistsInMultipleLocations_OnlyUpdatesTargetLocation()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Design Patterns",
            Description = "Livre",
            Price = 55m,
            AlertQuantity = 4,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location quebec = new()
        {
            Title = "Quebec City Branch",
            Address = "123 Rue Test",
            Description = "Magasin Québec"
        };

        Location levis = new()
        {
            Title = "Levis Branch",
            Address = "789 Rue Test",
            Description = "Magasin Lévis"
        };

        context.Items.Add(item);
        context.Locations.AddRange(quebec, levis);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = quebec.Id,
                Quantity = 8
            },
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = levis.Id,
                Quantity = 30
            });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = levis.Id,
            Quantity = 3
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? quebecLine = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == quebec.Id);

        InventoryLine? levisLine = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == levis.Id);

        Assert.NotNull(quebecLine);
        Assert.NotNull(levisLine);

        Assert.Equal(8, quebecLine.Quantity);
        Assert.Equal(3, levisLine.Quantity);
    }

    [Fact]
    public async Task AddQuantity_WhenItemExistsInAnotherLocation_CreatesNewInventoryLine()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Clean Code",
            Price = 30,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location quebec = new() { Title = "QC", Address = "A", Description = "A" };
        Location montreal = new() { Title = "MTL", Address = "B", Description = "B" };

        context.Items.Add(item);
        context.Locations.AddRange(quebec, montreal);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = quebec.Id,
            Quantity = 10
        });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = montreal.Id,
            Quantity = 5
        };

        await m_client.PostAsJsonAsync("/api/inventory/quantity/add", request);

        context.ChangeTracker.Clear();

        int count = context.InventoryLines.Count(p_x => p_x.ItemId == item.Id);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task UpdateQuantity_ToZero_SetsQuantityToZero()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "ZeroTest",
            Price = 10,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location loc = new() { Title = "QC", Address = "A", Description = "A" };

        context.Items.Add(item);
        context.Locations.Add(loc);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = loc.Id,
            Quantity = 10
        });

        await context.SaveChangesAsync();

        await m_client.PutAsJsonAsync("/api/inventory/quantity", new UpdateInventoryQuantityRequest
        {
            ItemId = item.Id,
            LocationId = loc.Id,
            Quantity = 0
        });

        context.ChangeTracker.Clear();

        int qty = context.InventoryLines
            .First(p_x => p_x.ItemId == item.Id && p_x.LocationId == loc.Id)
            .Quantity;

        Assert.Equal(0, qty);
    }

    [Fact]
    public async Task AddQuantity_DoesNotCreateDuplicateInventoryLine()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Test",
            Price = 10,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location loc = new() { Title = "QC", Address = "A", Description = "A" };

        context.Items.Add(item);
        context.Locations.Add(loc);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = loc.Id,
            Quantity = 5
        });

        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = item.Id,
            LocationId = loc.Id,
            Quantity = 3
        };

        await m_client.PostAsJsonAsync("/api/inventory/quantity/add", request);

        context.ChangeTracker.Clear();

        int count = context.InventoryLines.Count(p_x => p_x.ItemId == item.Id && p_x.LocationId == loc.Id);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AddQuantity_WithInvalidItem_ReturnsBadRequest()
    {
        await AuthenticateAsync();
        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = 999,
            LocationId = 999,
            Quantity = 5
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/inventory/quantity/add", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetStock_WhenInventoryLineDoesNotExist_CreatesLine()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Produit stock succursale",
            Price = 10m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Branch A",
            Address = "1 rue Test",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        UpdateStockRequest request = new() { Quantity = 12 };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            $"/api/inventory/locations/{location.Id}/items/{item.Id}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(12, line.Quantity);
    }

    [Fact]
    public async Task SetStock_WhenInventoryLineExists_ReplacesQuantity()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Produit existant",
            Price = 15m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Branch B",
            Address = "2 rue Test",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 3
        });

        await context.SaveChangesAsync();

        UpdateStockRequest request = new() { Quantity = 20 };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            $"/api/inventory/locations/{location.Id}/items/{item.Id}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(20, line.Quantity);
    }

    [Fact]
    public async Task SetStock_WithUnknownItem_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        UpdateStockRequest request = new() { Quantity = 5 };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            "/api/inventory/locations/1/items/999999",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ReturnsAllLines_WhenNoLocationFilter()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item itemA = new()
        {
            Name = "Article A",
            Price = 10m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item itemB = new()
        {
            Name = "Article B",
            Price = 20m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location locationA = new()
        {
            Title = "Branch A",
            Address = "1 rue A",
            Description = "Test A"
        };

        Location locationB = new()
        {
            Title = "Branch B",
            Address = "2 rue B",
            Description = "Test B"
        };

        context.Items.AddRange(itemA, itemB);
        context.Locations.AddRange(locationA, locationB);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = itemA.Id, LocationId = locationA.Id, Quantity = 3 },
            new InventoryLine { ItemId = itemB.Id, LocationId = locationB.Id, Quantity = 7 });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/inventory");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LocationInventoryLineResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LocationInventoryLineResponseDto>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Contains(body, p_line =>
            p_line.LocationId == locationA.Id &&
            p_line.LocationTitle == "Branch A" &&
            p_line.ItemId == itemA.Id &&
            p_line.ItemName == "Article A" &&
            p_line.Quantity == 3);
        Assert.Contains(body, p_line =>
            p_line.LocationId == locationB.Id &&
            p_line.LocationTitle == "Branch B" &&
            p_line.ItemId == itemB.Id &&
            p_line.ItemName == "Article B" &&
            p_line.Quantity == 7);
    }

    [Fact]
    public async Task GetInventory_FiltersByLocationId_WhenProvided()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Article filtre",
            Price = 15m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location targetLocation = new()
        {
            Title = "Target Branch",
            Address = "10 rue Cible",
            Description = "Test"
        };

        Location otherLocation = new()
        {
            Title = "Autre succursale",
            Address = "20 rue Autre",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.AddRange(targetLocation, otherLocation);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = item.Id, LocationId = targetLocation.Id, Quantity = 4 },
            new InventoryLine { ItemId = item.Id, LocationId = otherLocation.Id, Quantity = 9 });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory?p_locationId={targetLocation.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LocationInventoryLineResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LocationInventoryLineResponseDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(targetLocation.Id, body[0].LocationId);
        Assert.Equal("Target Branch", body[0].LocationTitle);
        Assert.Equal(item.Id, body[0].ItemId);
        Assert.Equal("Article filtre", body[0].ItemName);
        Assert.Equal(4, body[0].Quantity);
    }

    [Fact]
    public async Task GetInventory_WithUnknownLocation_ReturnsNotFound()
    {
        await AuthenticateAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/inventory?p_locationId=999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_FiltersByItemId_WhenProvided()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item targetItem = new()
        {
            Name = "Article cible",
            Price = 12m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item otherItem = new()
        {
            Name = "Autre article",
            Price = 18m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location locationA = new()
        {
            Title = "Branch A",
            Address = "1 rue A",
            Description = "Test"
        };

        Location locationB = new()
        {
            Title = "Branch B",
            Address = "2 rue B",
            Description = "Test"
        };

        context.Items.AddRange(targetItem, otherItem);
        context.Locations.AddRange(locationA, locationB);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = targetItem.Id, LocationId = locationA.Id, Quantity = 5 },
            new InventoryLine { ItemId = targetItem.Id, LocationId = locationB.Id, Quantity = 8 },
            new InventoryLine { ItemId = otherItem.Id, LocationId = locationA.Id, Quantity = 99 });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory?p_itemId={targetItem.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LocationInventoryLineResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LocationInventoryLineResponseDto>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.All(body, p_line => Assert.Equal(targetItem.Id, p_line.ItemId));
        Assert.All(body, p_line => Assert.Equal("Article cible", p_line.ItemName));
        Assert.Contains(body, p_line => p_line.LocationId == locationA.Id && p_line.Quantity == 5);
        Assert.Contains(body, p_line => p_line.LocationId == locationB.Id && p_line.Quantity == 8);
    }

    [Fact]
    public async Task GetInventory_FiltersByLocationAndItemId_WhenBothProvided()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Article combiné",
            Price = 10m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location targetLocation = new()
        {
            Title = "Target Branch",
            Address = "10 rue Cible",
            Description = "Test"
        };

        Location otherLocation = new()
        {
            Title = "Autre succursale",
            Address = "20 rue Autre",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.AddRange(targetLocation, otherLocation);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = item.Id, LocationId = targetLocation.Id, Quantity = 3 },
            new InventoryLine { ItemId = item.Id, LocationId = otherLocation.Id, Quantity = 11 });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory?p_locationId={targetLocation.Id}&p_itemId={item.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LocationInventoryLineResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LocationInventoryLineResponseDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(targetLocation.Id, body[0].LocationId);
        Assert.Equal(item.Id, body[0].ItemId);
        Assert.Equal(3, body[0].Quantity);
    }

    [Fact]
    public async Task GetInventory_WithUnknownItem_ReturnsNotFound()
    {
        await AuthenticateAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/inventory?p_itemId=999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStock_WhenInventoryLineExists_ReturnsQuantity()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Produit lecture stock",
            Price = 12m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Read Branch",
            Address = "3 rue Test",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = item.Id,
            LocationId = location.Id,
            Quantity = 7
        });

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory/locations/{location.Id}/items/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        InventoryStockResponseDto? body =
            await response.Content.ReadFromJsonAsync<InventoryStockResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(location.Id, body.LocationId);
        Assert.Equal(item.Id, body.ItemId);
        Assert.Equal(7, body.Quantity);
    }

    [Fact]
    public async Task GetStock_WhenInventoryLineDoesNotExist_ReturnsZero()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Produit sans ligne",
            Price = 8m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Empty Branch",
            Address = "4 rue Test",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory/locations/{location.Id}/items/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        InventoryStockResponseDto? body =
            await response.Content.ReadFromJsonAsync<InventoryStockResponseDto>();

        Assert.NotNull(body);
        Assert.Equal(location.Id, body.LocationId);
        Assert.Equal(item.Id, body.ItemId);
        Assert.Equal(0, body.Quantity);
    }

    [Fact]
    public async Task GetStock_WithUnknownItem_ReturnsNotFound()
    {
        await AuthenticateAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            "/api/inventory/locations/1/items/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ExcludesInactiveCatalogItems()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Location location = await context.Locations.FirstAsync();
        Item inactiveItem = new()
        {
            Name = "bbb",
            Price = 1m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = false,
        };

        context.Items.Add(inactiveItem);
        await context.SaveChangesAsync();

        context.InventoryLines.Add(new InventoryLine
        {
            ItemId = inactiveItem.Id,
            LocationId = location.Id,
            Quantity = 1,
        });
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync(
            $"/api/inventory?p_locationId={location.Id}");

        response.EnsureSuccessStatusCode();

        List<LocationInventoryLineResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LocationInventoryLineResponseDto>>();

        Assert.NotNull(body);
        Assert.DoesNotContain(body, p_line => p_line.ItemId == inactiveItem.Id);
    }

    [Fact]
    public async Task UpdateQuantity_ForInactiveItem_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Location location = await context.Locations.FirstAsync();
        Item inactiveItem = new()
        {
            Name = "Article inactif",
            Price = 1m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = false,
        };

        context.Items.Add(inactiveItem);
        await context.SaveChangesAsync();

        UpdateInventoryQuantityRequest request = new()
        {
            ItemId = inactiveItem.Id,
            LocationId = location.Id,
            Quantity = 3,
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ImportExcel_WithValidFile_UpdatesInventoryLines()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Import Excel",
            Price = 5m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location location = new()
        {
            Title = "Import succursale",
            Address = "3 rue Test",
            Description = "Test"
        };

        context.Items.Add(item);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        List<InventoryExcelRow> excelRows = new()
        {
            new InventoryExcelRow
            {
                LocationId = location.Id,
                ItemId = item.Id,
                Quantity = 42
            }
        };

        MemoryStream memoryStream = new();
        await memoryStream.SaveAsAsync(excelRows);
        memoryStream.Position = 0;

        using MultipartFormDataContent formContent = new();
        StreamContent fileContent = new(memoryStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        formContent.Add(fileContent, "p_file", "inventory-import.xlsx");

        HttpResponseMessage response = await m_client.PostAsync("/api/inventory/import", formContent);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        InventoryLine? line = context.InventoryLines
            .FirstOrDefault(p_x => p_x.ItemId == item.Id && p_x.LocationId == location.Id);

        Assert.NotNull(line);
        Assert.Equal(42, line.Quantity);
    }

    [Fact]
    public async Task ImportExcel_WithInvalidExtension_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        using MultipartFormDataContent formContent = new();
        ByteArrayContent textContent = new(System.Text.Encoding.UTF8.GetBytes("not excel"));
        textContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        formContent.Add(textContent, "p_file", "inventory.txt");

        HttpResponseMessage response = await m_client.PostAsync("/api/inventory/import", formContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateQuantity_Returns403_WhenEmployeeHasNoInventoryUpdatePermission()
    {
        try
        {
            InventoryTestSeedResult seed = await SeedInventoryTestDataAsync();
            await ResetEmployeeDynamicRoleAsync();
            await AuthenticateAsEmployeeAsync();

            UpdateInventoryQuantityRequest request = new()
            {
                ItemId = seed.ItemId,
                LocationId = seed.AllowedLocationId,
                Quantity = 5,
            };

            HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await CleanupEmployeeInventoryTestRoleAsync();
        }
    }

    [Fact]
    public async Task UpdateQuantity_Returns204_WhenEmployeeHasSpecificScopeOnAllowedLocation()
    {
        string? roleId = null;
        try
        {
            InventoryTestSeedResult seed = await SeedInventoryTestDataAsync();
            roleId = await AssignEmployeeInventoryRoleAsync(LocationScopes.Specific, [seed.AllowedLocationId]);
            await AuthenticateAsEmployeeAsync();

            UpdateInventoryQuantityRequest request = new()
            {
                ItemId = seed.ItemId,
                LocationId = seed.AllowedLocationId,
                Quantity = 12,
            };

            HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await CleanupEmployeeInventoryTestRoleAsync(roleId);
        }
    }

    [Fact]
    public async Task UpdateQuantity_Returns403_WhenEmployeeHasSpecificScopeOnOtherLocation()
    {
        string? roleId = null;
        try
        {
            InventoryTestSeedResult seed = await SeedInventoryTestDataAsync();
            roleId = await AssignEmployeeInventoryRoleAsync(LocationScopes.Specific, [seed.AllowedLocationId]);
            await AuthenticateAsEmployeeAsync();

            UpdateInventoryQuantityRequest request = new()
            {
                ItemId = seed.ItemId,
                LocationId = seed.DeniedLocationId,
                Quantity = 12,
            };

            HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await CleanupEmployeeInventoryTestRoleAsync(roleId);
        }
    }

    [Fact]
    public async Task UpdateQuantity_Returns204_WhenEmployeeHasAllScopeOnAnyLocation()
    {
        string? roleId = null;
        try
        {
            InventoryTestSeedResult seed = await SeedInventoryTestDataAsync();
            roleId = await AssignEmployeeInventoryRoleAsync(LocationScopes.All, []);
            await AuthenticateAsEmployeeAsync();

            UpdateInventoryQuantityRequest request = new()
            {
                ItemId = seed.ItemId,
                LocationId = seed.DeniedLocationId,
                Quantity = 20,
            };

            HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/inventory/quantity", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await CleanupEmployeeInventoryTestRoleAsync(roleId);
        }
    }

    private async Task AuthenticateAsync()
    {
        LoginRequest request = new()
        {
            Email = "admin@crystal.local",
            Password = "ValidPass1!a"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        m_client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);
    }

    private async Task AuthenticateAsEmployeeAsync()
    {
        LoginRequest request = new()
        {
            Email = "employee@crystal.local",
            Password = "ValidPass1!a",
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    private async Task ResetEmployeeDynamicRoleAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? employee = await userManager.FindByEmailAsync("employee@crystal.local");
        if (employee is not null)
        {
            employee.DynamicRoleId = ApplicationRoles.Employee;
            await userManager.UpdateAsync(employee);
        }
    }

    private async Task<string> AssignEmployeeInventoryRoleAsync(string p_locationScope, IReadOnlyList<int> p_locationIds)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? employee = await userManager.FindByEmailAsync("employee@crystal.local");
        Assert.NotNull(employee);

        string roleId = $"test-inventory-{Guid.NewGuid():N}";

        RolePermission rolePermission = new()
        {
            Action = PermissionActions.Update,
            Subject = PermissionSubjects.InventoryQuantity,
            LocationScope = p_locationScope,
        };

        if (p_locationScope == LocationScopes.Specific)
        {
            foreach (int locationId in p_locationIds)
            {
                rolePermission.ScopedLocations.Add(new RolePermissionLocation
                {
                    LocationId = locationId,
                });
            }
        }

        DynamicRole role = new()
        {
            Id = roleId,
            Name = "Inventory Test Employee",
            IsPreset = false,
            Permissions = [rolePermission],
        };

        await context.DynamicRoles.AddAsync(role);
        await context.SaveChangesAsync();

        employee.DynamicRoleId = roleId;
        await userManager.UpdateAsync(employee);

        return roleId;
    }

    private async Task CleanupEmployeeInventoryTestRoleAsync(string? p_roleId = null)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? employee = await userManager.FindByEmailAsync("employee@crystal.local");
        if (employee is not null)
        {
            string? roleIdToDelete = p_roleId ?? employee.DynamicRoleId;
            if (!string.IsNullOrWhiteSpace(roleIdToDelete) && roleIdToDelete.StartsWith("test-inventory-", StringComparison.Ordinal))
            {
                if (employee.DynamicRoleId == roleIdToDelete)
                {
                    employee.DynamicRoleId = ApplicationRoles.Employee;
                    await userManager.UpdateAsync(employee);
                }

                DynamicRole? role = await context.DynamicRoles
                    .Include(p_dynamicRole => p_dynamicRole.Permissions)
                    .ThenInclude(p_permission => p_permission.ScopedLocations)
                    .FirstOrDefaultAsync(p_dynamicRole => p_dynamicRole.Id == roleIdToDelete);

                if (role is not null)
                {
                    context.DynamicRoles.Remove(role);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    private async Task<InventoryTestSeedResult> SeedInventoryTestDataAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Item item = new()
        {
            Name = $"Article scope test {Guid.NewGuid():N}",
            Description = "Test",
            Price = 19.99m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
        };

        Location allowedLocation = new()
        {
            Title = $"Authorized Branch {Guid.NewGuid():N}",
            Address = "1 rue A",
            Description = "Test A",
        };

        Location deniedLocation = new()
        {
            Title = $"Denied Branch {Guid.NewGuid():N}",
            Address = "2 rue B",
            Description = "Test B",
        };

        context.Items.Add(item);
        context.Locations.AddRange(allowedLocation, deniedLocation);
        await context.SaveChangesAsync();

        return new InventoryTestSeedResult(item.Id, allowedLocation.Id, deniedLocation.Id);
    }

    private sealed record InventoryTestSeedResult(int ItemId, int AllowedLocationId, int DeniedLocationId);

    public void Dispose()
    {
        m_client.Dispose();
    }
}