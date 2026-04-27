using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Crystal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Crystal.UnitTests.Services;

public class ItemServiceTests
{
    [Fact]
    public async Task GetAllItemsAsync_ReturnsOnlyActiveItems()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();

        Item activeItem = new Item
        {
            Name = "Active item",
            Description = "Visible item",
            Price = 12.5m,
            AlertQuantity = 3,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item inactiveItem = new Item
        {
            Name = "Inactive item",
            Description = "Hidden item",
            Price = 20m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = false
        };

        context.Items.Add(activeItem);
        context.Items.Add(inactiveItem);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);

        // Act
        IEnumerable<ItemResponse> result = await service.GetAllItemsAsync(CancellationToken.None);
        List<ItemResponse> resultList = result.ToList();

        // Assert
        Assert.Single(resultList);
        Assert.Equal("Active item", resultList[0].Name);
    }

    [Fact]
    public async Task GetAllItemsAsync_CalculatesCorrectTotalQuantity()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();

        Item item = new Item
        {
            Name = "Stock item",
            Description = "Item with inventory lines",
            Price = 9.99m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        InventoryLine firstLine = new InventoryLine
        {
            ItemId = item.Id,
            LocationId = 1,
            Quantity = 10
        };

        InventoryLine secondLine = new InventoryLine
        {
            ItemId = item.Id,
            LocationId = 2,
            Quantity = -3
        };

        context.InventoryLines.Add(firstLine);
        context.InventoryLines.Add(secondLine);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);

        // Act
        IEnumerable<ItemResponse> result = await service.GetAllItemsAsync(CancellationToken.None);
        ItemResponse itemResponse = Assert.Single(result);

        // Assert
        Assert.Equal(7, itemResponse.TotalQuantity);
    }

    [Fact]
    public async Task GetItemByIdAsync_ActiveItem_ReturnsItemResponse()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();

        Item item = new Item
        {
            Name = "Detail active item",
            Description = "Item for details endpoint",
            Price = 14.25m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        InventoryLine firstLine = new InventoryLine
        {
            ItemId = item.Id,
            LocationId = 11,
            Quantity = 5
        };

        InventoryLine secondLine = new InventoryLine
        {
            ItemId = item.Id,
            LocationId = 12,
            Quantity = -1
        };

        context.InventoryLines.Add(firstLine);
        context.InventoryLines.Add(secondLine);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);

        // Act
        ItemResponse? result = await service.GetItemByIdAsync(item.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(item.Id, result.Id);
        Assert.Equal("Detail active item", result.Name);
        Assert.Equal(4, result.TotalQuantity);
    }

    [Fact]
    public async Task GetItemByIdAsync_InactiveItem_ReturnsNull()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();

        Item item = new Item
        {
            Name = "Inactive detail item",
            Description = "Inactive details",
            Price = 8.75m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = false
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);

        // Act
        ItemResponse? result = await service.GetItemByIdAsync(item.Id, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        ItemService service = new ItemService(context);

        // Act
        ItemResponse? result = await service.GetItemByIdAsync(999999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateItemAsync_ValidRequest_CreatesItemAndReturnsResponse()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        ItemService service = new ItemService(context);

        CreateItemRequest request = new CreateItemRequest
        {
            Name = "Created item",
            Description = "Created through service test",
            Price = 19.99m,
            AlertQuantity = 6,
            InitialQuantity = 0
        };

        // Act
        ItemResponse result = await service.CreateItemAsync(request, CancellationToken.None);
        Item? itemInDatabase = await context.Items.FirstOrDefaultAsync(i => i.Id == result.Id);

        // Assert
        Assert.NotNull(itemInDatabase);
        Assert.Equal("Created item", itemInDatabase.Name);
        Assert.True(itemInDatabase.IsActive);
        Assert.Equal("Created item", result.Name);
    }

    [Fact]
    public async Task CreateItemAsync_WithInitialQuantity_CreatesInventoryLine()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        context.Locations.Add(new Location
        {
            Title = "Main location",
            Address = "123 Main street",
            Description = "Default location for tests"
        });
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);
        CreateItemRequest request = new CreateItemRequest
        {
            Name = "Stocked item",
            Description = "Item with initial stock",
            Price = 29.99m,
            AlertQuantity = 5,
            InitialQuantity = 10
        };

        // Act
        ItemResponse result = await service.CreateItemAsync(request, CancellationToken.None);
        InventoryLine? inventoryLine = await context.InventoryLines.FirstOrDefaultAsync(i => i.ItemId == result.Id);

        // Assert
        Assert.NotNull(inventoryLine);
        Assert.Equal(10, inventoryLine.Quantity);
        Assert.Equal(10, result.TotalQuantity);
    }

    [Fact]
    public async Task UpdateItemAsync_ValidRequest_UpdatesAndReturnsItem()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        Item item = new Item
        {
            Name = "Original name",
            Description = "Original description",
            Price = 12m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow.AddHours(-2),
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        DateTime previousLastUpdate = item.LastUpdate;
        ItemService service = new ItemService(context);
        UpdateItemRequest request = new UpdateItemRequest
        {
            Name = "Updated name",
            Description = "Updated description",
            Price = 17m,
            AlertQuantity = 5,
            BookId = null
        };

        // Act
        ItemResponse? result = await service.UpdateItemAsync(item.Id, request, CancellationToken.None);
        Item? updatedInDatabase = await context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(updatedInDatabase);
        Assert.Equal("Updated name", result.Name);
        Assert.Equal("Updated name", updatedInDatabase.Name);
        Assert.True(updatedInDatabase.LastUpdate > previousLastUpdate);
    }

    [Fact]
    public async Task UpdateItemAsync_ItemNotFoundOrInactive_ReturnsNull()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        Item inactiveItem = new Item
        {
            Name = "Inactive item for update",
            Description = "Inactive",
            Price = 11m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = false
        };

        context.Items.Add(inactiveItem);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);
        UpdateItemRequest request = new UpdateItemRequest
        {
            Name = "Should not update",
            Description = "No update",
            Price = 22m,
            AlertQuantity = 3,
            BookId = null
        };

        // Act
        ItemResponse? notFoundResult = await service.UpdateItemAsync(999999, request, CancellationToken.None);
        ItemResponse? inactiveResult = await service.UpdateItemAsync(inactiveItem.Id, request, CancellationToken.None);

        // Assert
        Assert.Null(notFoundResult);
        Assert.Null(inactiveResult);
    }

    [Fact]
    public async Task DeleteItemAsync_ValidId_SetsIsActiveToFalse()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        Item item = new Item
        {
            Name = "Delete target",
            Description = "Active item to delete",
            Price = 20m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow.AddHours(-1),
            IsActive = true
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        ItemService service = new ItemService(context);

        // Act
        bool result = await service.DeleteItemAsync(item.Id, CancellationToken.None);
        Item? deletedItem = await context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedItem);
        Assert.False(deletedItem.IsActive);
    }

    [Fact]
    public async Task DeleteItemAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        await using CrystalDbContext context = CreateInMemoryDbContext();
        ItemService service = new ItemService(context);

        // Act
        bool result = await service.DeleteItemAsync(999999, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    private static CrystalDbContext CreateInMemoryDbContext()
    {
        string databaseName = Guid.NewGuid().ToString();
        DbContextOptions<CrystalDbContext> options = new DbContextOptionsBuilder<CrystalDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        CrystalDbContext context = new CrystalDbContext(options);
        return context;
    }
}
