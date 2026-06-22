using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Crystal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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
        await AuthenticateAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetInventory_ReturnsItemFields()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new()
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
        await AuthenticateAsync();
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
        Assert.Contains(body, p_i => p_i.Name == "Item actif");
        Assert.DoesNotContain(body, p_i => p_i.Name == "Item inactif");
    }

    [Fact]
    public async Task GetInventory_WithIsBookTrue_ReturnsOnlyBooks()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item genericItem = new()
        {
            Name = "Clavier générique",
            Price = 50m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item bookItem = new()
        {
            Name = "Test Novel",
            Price = 15m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Crystal.Core.Entities.Book
            {
                PublicationDate = new DateOnly(2022, 3, 10)
            }
        };

        context.Items.AddRange(genericItem, bookItem);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/items?p_isBook=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ItemResponseDto>? body = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Test Novel", body[0].Name);
        Assert.True(body[0].IsBook);
    }

    [Fact]
    public async Task GetInventory_WithIsBookFalse_ReturnsOnlyGenericItems()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item genericItem = new()
        {
            Name = "Generic Mouse",
            Price = 30m,
            AlertQuantity = 3,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item bookItem = new()
        {
            Name = "Essai test",
            Price = 20m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Crystal.Core.Entities.Book
            {
                PublicationDate = new DateOnly(2021, 1, 1)
            }
        };

        context.Items.AddRange(genericItem, bookItem);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/items?p_isBook=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ItemResponseDto>? body = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Generic Mouse", body[0].Name);
        Assert.False(body[0].IsBook);
    }

    [Fact]
    public async Task GetItemById_WithExistingItem_ReturnsItem()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Domain Driven Design",
            Description = "DDD book",
            Price = 45m,
            AlertQuantity = 3,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync($"/api/items/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ItemResponseDto? dto = await response.Content.ReadFromJsonAsync<ItemResponseDto>();

        Assert.NotNull(dto);
        Assert.Equal(item.Id, dto.Id);
        Assert.Equal("Domain Driven Design", dto.Name);
    }

    [Fact]
    public async Task GetItemById_WithUnknownId_ReturnsNotFound()
    {
        await AuthenticateAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/items/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItemById_WithInactiveItem_ReturnsNotFound()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Hidden Item",
            Description = "Should not be visible",
            Price = 10m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = false
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync($"/api/items/{item.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItemById_ReturnsComputedStockFields()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.InventoryLines.RemoveRange(context.InventoryLines);
        context.Items.RemoveRange(context.Items);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Location location1 = new()
        {
            Title = "Entrepôt test 1",
            Address = "123 rue test",
            Description = "Location de test 1"
        };

        Location location2 = new()
        {
            Title = "Entrepôt test 2",
            Address = "456 rue test",
            Description = "Location de test 2"
        };

        Item item = new()
        {
            Name = "Item stock test",
            Price = 10m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Locations.AddRange(location1, location2);
        context.Items.Add(item);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        context.InventoryLines.AddRange(
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = location1.Id,
                Quantity = 2
            },
            new InventoryLine
            {
                ItemId = item.Id,
                LocationId = location2.Id,
                Quantity = 3
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        HttpResponseMessage response = await m_client.GetAsync($"/api/items/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ItemResponseDto? dto = await response.Content.ReadFromJsonAsync<ItemResponseDto>();

        Assert.NotNull(dto);
        Assert.Equal(5, dto.TotalQuantity);
        Assert.True(dto.IsLowStock);
    }

    [Fact]
    public async Task GetBookById_WithUnknownId_ReturnsNotFound()
    {
        await AuthenticateAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/books/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_WithValidRequest_ReturnsCreatedItem()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        CreateItemRequest request = new()
        {
            Name = "Clean Code",
            Description = "Livre de programmation",
            Price = 32.50m,
            AlertQuantity = 5
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items", request);

        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ItemResponseDto? body = await response.Content.ReadFromJsonAsync<ItemResponseDto>();

        Assert.NotNull(body);
        Assert.Equal("Clean Code", body.Name);
        Assert.Equal("Livre de programmation", body.Description);
        Assert.Equal(32.50m, body.Price);
        Assert.Equal(5, body.AlertQuantity);
    }

    [Fact]
    public async Task CreateItem_WithDuplicateName_Returns400()
    {
        await AuthenticateAsync();
        CreateItemRequest request = new()
        {
            Name = "Duplicate",
            Description = "Test",
            Price = 10,
            AlertQuantity = 1
        };

        HttpResponseMessage first = await m_client.PostAsJsonAsync("/api/items", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        HttpResponseMessage second = await m_client.PostAsJsonAsync("/api/items", request);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task CreateItem_AfterSoftDelete_AllowsSameName()
    {
        await AuthenticateAsync();

        CreateItemRequest request = new()
        {
            Name = "Article recycle",
            Description = "Première version",
            Price = 10,
            AlertQuantity = 1
        };

        HttpResponseMessage created = await m_client.PostAsJsonAsync("/api/items", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        ItemResponseDto? createdBody = await created.Content.ReadFromJsonAsync<ItemResponseDto>();
        Assert.NotNull(createdBody);

        HttpResponseMessage deleted =
            await m_client.DeleteAsync($"/api/items/{createdBody!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        CreateItemRequest recycledRequest = new()
        {
            Name = "Article recycle",
            Description = "Nouvelle version",
            Price = 12,
            AlertQuantity = 2
        };

        HttpResponseMessage recreated = await m_client.PostAsJsonAsync("/api/items", recycledRequest);
        Assert.Equal(HttpStatusCode.Created, recreated.StatusCode);

        ItemResponseDto? recreatedBody = await recreated.Content.ReadFromJsonAsync<ItemResponseDto>();
        Assert.NotNull(recreatedBody);
        Assert.NotEqual(createdBody.Id, recreatedBody!.Id);
        Assert.Equal("Nouvelle version", recreatedBody.Description);
    }

    [Fact]
    public async Task CreateBook_WithValidRequest_ReturnsCreatedItem()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        CreateBookRequest request = new()
        {
            Name = "Clean Code",
            Description = "Livre de programmation",
            Price = 32.50m,
            AlertQuantity = 5,
            Isbn = "978-0132350884",
            PublicationDate = new DateOnly(2008, 8, 1),
            Publishers = ["Prentice Hall"]
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        BookResponseDto? body = await response.Content.ReadFromJsonAsync<BookResponseDto>();

        Assert.NotNull(body);
        Assert.Equal("Clean Code", body.Name);
        Assert.Equal("978-0132350884", body.Isbn);
        Assert.Contains("Prentice Hall", body.Publishers);
        Assert.Equal("Livre de programmation", body.Description);
        Assert.Equal(32.50m, body.Price);
        Assert.Equal(5, body.AlertQuantity);
    }

    [Fact]
    public async Task CreateBook_WithValidRequest_CreatesItemAndBookInDatabase()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        CreateBookRequest request = new()
        {
            Name = "Domain-Driven Design",
            Description = "Livre de conception logicielle",
            Price = 45.99m,
            AlertQuantity = 3,
            Isbn = "978-0321125217",
            PublicationDate = new DateOnly(2003, 8, 30)
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/items/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Item? item = await context.Items
            .Include(p_i => p_i.Book)
            .FirstOrDefaultAsync(p_i => p_i.Name == "Domain-Driven Design");

        Assert.NotNull(item);
        Assert.NotNull(item.Book);
        Assert.Equal("Domain-Driven Design", item.Name);
        Assert.Equal(45.99m, item.Price);
        Assert.Equal(new DateOnly(2003, 8, 30), item.Book.PublicationDate);
    }

    [Fact]
    public async Task GetInventory_WhenItemInMultipleLocations_TotalQuantityIsSum()
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
            Name = "DDD",
            Price = 50,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location a = new() { Title = "A", Address = "A", Description = "A" };
        Location b = new() { Title = "B", Address = "B", Description = "B" };

        context.Items.Add(item);
        context.Locations.AddRange(a, b);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = item.Id, LocationId = a.Id, Quantity = 4 },
            new InventoryLine { ItemId = item.Id, LocationId = b.Id, Quantity = 6 }
        );

        await context.SaveChangesAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        List<ItemResponseDto>? items = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();
        ItemResponseDto dto = Assert.Single(items!);

        Assert.Equal(10, dto.TotalQuantity);
    }

    [Fact]
    public async Task GetInventory_WhenTotalBelowAlert_IsLowStockTrue()
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
            Price = 40,
            AlertQuantity = 10,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Location loc = new() { Title = "A", Address = "A", Description = "A" };

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

        HttpResponseMessage response = await m_client.GetAsync("/api/items");

        List<ItemResponseDto>? items = await response.Content.ReadFromJsonAsync<List<ItemResponseDto>>();
        Assert.NotNull(items);
        ItemResponseDto dto = Assert.Single(items);

        Assert.True(dto.IsLowStock);
    }

    [Fact]
    public async Task UpdateItem_WithValidRequest_ReturnsUpdatedItem()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Old Name",
            Description = "Old",
            Price = 10,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        UpdateItemRequest request = new()
        {
            Name = "New Name",
            Description = "New",
            Price = 20,
            AlertQuantity = 5,
            IsActive = true
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync($"/api/items/{item.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ItemResponseDto? body =
            await response.Content.ReadFromJsonAsync<ItemResponseDto>();

        Assert.NotNull(body);
        Assert.Equal("New Name", body.Name);
        Assert.Equal(20, body.Price);
    }

    [Fact]
    public async Task CleanupDuplicateCatalogItems_MergesInventoryAndDeactivatesDuplicates()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Location location = await context.Locations.FirstAsync();

        Item canonicalItem = new()
        {
            Name = "Cahier Moleskine édition Québec",
            Price = 24.99m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
        };

        Item duplicateItem = new()
        {
            Name = "Cahier Moleskine édition Québec",
            Price = 24.99m,
            AlertQuantity = 5,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
        };

        context.Items.AddRange(canonicalItem, duplicateItem);
        await context.SaveChangesAsync();

        context.InventoryLines.AddRange(
            new InventoryLine { ItemId = canonicalItem.Id, LocationId = location.Id, Quantity = 22 },
            new InventoryLine { ItemId = duplicateItem.Id, LocationId = location.Id, Quantity = 15 });
        await context.SaveChangesAsync();

        await DataSeeder.CleanupDuplicateCatalogItemsAsync(scope.ServiceProvider);

        context.ChangeTracker.Clear();

        List<Item> activeItems = await context.Items
            .Where(p_item => p_item.IsActive && p_item.Name == "Cahier Moleskine édition Québec")
            .ToListAsync();

        Assert.Single(activeItems);

        InventoryLine? mergedLine = await context.InventoryLines
            .SingleOrDefaultAsync(p_line =>
                p_line.ItemId == activeItems[0].Id &&
                p_line.LocationId == location.Id);

        Assert.NotNull(mergedLine);
        Assert.Equal(37, mergedLine!.Quantity);
        Assert.False(await context.Items.AnyAsync(p_item => p_item.Id == duplicateItem.Id && p_item.IsActive));
    }

    [Fact]
    public async Task UpdateItem_WithoutIsActive_KeepsItemActiveInCatalog()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Item item = new()
        {
            Name = "Sac réutilisable",
            Price = 8.99m,
            AlertQuantity = 10,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        UpdateItemRequest request = new()
        {
            Name = "Sac réutilisable Librairie Crystal",
            Description = "Sac en coton",
            Price = 8.99m,
            AlertQuantity = 4
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync($"/api/items/{item.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        context.ChangeTracker.Clear();

        Item? dbItem = await context.Items.FindAsync(item.Id);
        Assert.NotNull(dbItem);
        Assert.True(dbItem.IsActive);
        Assert.Equal(4, dbItem.AlertQuantity);

        HttpResponseMessage getResponse =
            await m_client.GetAsync($"/api/items/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithCategoryIds_ReplacesBookCategoriesInDatabase()
    {
        await AuthenticateAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.BookCategories.RemoveRange(context.BookCategories);
        context.Books.RemoveRange(context.Books);
        context.Items.RemoveRange(context.Items);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.Category categoryInitial = new()
        {
            Name = "Initial Category",
            IsDeleted = false
        };

        Crystal.Core.Entities.Category categoryReplacement = new()
        {
            Name = "Replacement Category",
            IsDeleted = false
        };

        context.Categories.AddRange(categoryInitial, categoryReplacement);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Livre catégories",
            Description = "Test",
            Price = 25m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Crystal.Core.Entities.Book
            {
                PublicationDate = new DateOnly(2019, 6, 1),
                BookCategories =
                [
                    new BookCategory { CategoryId = categoryInitial.Id }
                ]
            }
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        UpdateItemRequest request = new()
        {
            Name = "Livre catégories",
            Description = "Test",
            Price = 25m,
            AlertQuantity = 2,
            IsActive = true,
            CategoryIds = [categoryReplacement.Id]
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync($"/api/items/{item.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        context.ChangeTracker.Clear();

        List<BookCategory> bookCategories = await context.BookCategories
            .Where(p_bc => p_bc.BookId == item.Id)
            .ToListAsync();

        Assert.Single(bookCategories);
        Assert.Equal(categoryReplacement.Id, bookCategories[0].CategoryId);
    }

    [Fact]
    public async Task UpdateItem_WithInvalidId_Returns404()
    {
        await AuthenticateAsync();
        UpdateItemRequest request = new()
        {
            Name = "Test",
            Price = 10,
            AlertQuantity = 1,
            IsActive = true
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync("/api/items/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithInvalidData_Returns400()
    {
        await AuthenticateAsync();
        UpdateItemRequest request = new()
        {
            Name = "", // invalide
            Price = -10,
            AlertQuantity = -1,
            IsActive = true
        };

        HttpResponseMessage response =
            await m_client.PutAsJsonAsync("/api/items/1", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_WithValidId_Returns204()
    {
        await AuthenticateAsync();
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Item item = new()
        {
            Name = "ToDelete",
            Price = 10,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        HttpResponseMessage response =
            await m_client.DeleteAsync($"/api/items/{item.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        context.ChangeTracker.Clear();

        Item? dbItem = await context.Items.FindAsync(item.Id);
        Assert.False(dbItem!.IsActive);
    }

    [Fact]
    public async Task DeleteItem_WithInvalidId_Returns404()
    {
        await AuthenticateAsync();
        HttpResponseMessage response =
            await m_client.DeleteAsync("/api/items/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
}