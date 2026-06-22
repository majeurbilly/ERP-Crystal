using System.Collections.Generic;
using System.Linq;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Services;

public class ItemService : IItemService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png"
    };

    private const string ItemsImageFolder = "images/items";

    private readonly IItemRepository m_itemRepository;
    private readonly IBookRepository m_bookRepository;
    private readonly ICategoryRepository m_categoryRepository;
    private readonly IWebHostEnvironment m_webHostEnvironment;

    public ItemService(
        IItemRepository p_itemRepository,
        IBookRepository p_bookRepository,
        ICategoryRepository p_categoryRepository,
        IWebHostEnvironment p_webHostEnvironment)
    {
        m_itemRepository = p_itemRepository;
        m_bookRepository = p_bookRepository;
        m_categoryRepository = p_categoryRepository;
        m_webHostEnvironment = p_webHostEnvironment;
    }

    public async Task<ItemResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Item? item = await m_itemRepository.GetByIdReadOnlyAsync(p_id);

        if (item == null || !item.IsActive)
        {
            return null;
        }

        return MapToResponseDto(item);
    }

    public async Task<IEnumerable<ItemResponseDto>> GetInventoryAsync(
        string? p_search = null,
        int? p_publisherId = null,
        int[]? p_categoryIds = null,
        int? p_authorId = null,
        bool? p_isBook = null)
    {
        IQueryable<Item> query = m_itemRepository.QueryActiveReadOnly();

        if (p_isBook == true)
        {
            query = query.Where(p_item => p_item.Book != null);
        }
        else if (p_isBook == false)
        {
            query = query.Where(p_item => p_item.Book == null);
        }

        if (!string.IsNullOrWhiteSpace(p_search))
        {
            string search = p_search.Trim().ToLower();
            query = query.Where(p_item =>
                p_item.Name.ToLower().Contains(search) ||
                (p_item.Description != null && p_item.Description.ToLower().Contains(search)));
        }

        if (p_publisherId.HasValue)
        {
            int publisherId = p_publisherId.Value;
            query = query.Where(p_item =>
                p_item.Book != null &&
                p_item.Book.BookPublishers.Any(p_bookPublisher => p_bookPublisher.PublisherId == publisherId));
        }

        if (p_categoryIds is { Length: > 0 })
        {
            int[] categoryIds = p_categoryIds;
            query = query.Where(p_item =>
                p_item.Book != null &&
                p_item.Book.BookCategories.Any(p_bookCategory => categoryIds.Contains(p_bookCategory.CategoryId)));
        }

        if (p_authorId.HasValue)
        {
            int authorId = p_authorId.Value;
            query = query.Where(p_item =>
                p_item.Book != null &&
                p_item.Book.AuthorBooks.Any(p_authorBook => p_authorBook.AuthorId == authorId));
        }

        List<Item> items = await query.ToListAsync();

        return items.Select(MapToResponseDto);
    }

    public async Task<ItemResponseDto> CreateAsync(CreateItemRequest p_request)
    {
        if (string.IsNullOrWhiteSpace(p_request.Name))
        {
            throw new ArgumentException(ErrorMessages.Item.NameRequired);
        }

        if (p_request.Price < 0)
        {
            throw new ArgumentException(ErrorMessages.Item.NegativePrice);
        }

        if (p_request.AlertQuantity < 0)
        {
            throw new ArgumentException(ErrorMessages.Item.NegativeAlertQuantity);
        }

        if (await m_itemRepository.ExistsByNameAsync(p_request.Name))
        {
            throw new ArgumentException(ErrorMessages.Item.NameAlreadyExists);
        }

        Item item = new()
        {
            Name = p_request.Name.Trim(),
            Description = p_request.Description,
            Distributor = p_request.Distributor,
            Price = p_request.Price,
            AlertQuantity = p_request.AlertQuantity,
            LastUpdate = DateTime.UtcNow,
            IsActive = true
        };

        Item createdItem = await m_itemRepository.AddAsync(item);

        return MapToResponseDto(createdItem);
    }

    public async Task<ItemResponseDto> CreateBookAsync(CreateBookRequest p_request)
    {
        if (string.IsNullOrWhiteSpace(p_request.Name))
        {
            throw new ArgumentException(ErrorMessages.Item.BookNameRequired);
        }

        if (string.IsNullOrWhiteSpace(p_request.Isbn))
        {
            throw new ArgumentException(ErrorMessages.Item.IsbnRequired);
        }

        if (await m_itemRepository.ExistsByNameAsync(p_request.Name))
        {
            throw new ArgumentException(ErrorMessages.Item.NameAlreadyExists);
        }

        List<int> authorIds = await ResolveAuthorIdsAsync(p_request.AuthorIds, p_request.Authors);
        List<int> publisherIds = await ResolvePublisherIdsAsync(p_request.PublisherIds, p_request.Publishers);

        Item item = new()
        {
            Name = p_request.Name.Trim(),
            Description = p_request.Description,
            Distributor = p_request.Distributor,
            Price = p_request.Price,
            AlertQuantity = p_request.AlertQuantity,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Book
            {
                Isbn = p_request.Isbn.Trim(),
                PublicationDate = p_request.PublicationDate,
                AuthorBooks = authorIds
                    .Select(p_authorId => new AuthorBook { AuthorId = p_authorId })
                    .ToList(),
                BookCategories = p_request.CategoryIds
                    .Select(p_categoryId => new BookCategory { CategoryId = p_categoryId })
                    .ToList(),
                BookPublishers = publisherIds
                    .Select(p_publisherId => new BookPublisher { PublisherId = p_publisherId })
                    .ToList()
            }
        };

        await m_itemRepository.AddAsync(item);

        return await GetByIdAsync(item.Id)
            ?? throw new InvalidOperationException(ErrorMessages.Item.CreateLoadFailed);
    }

    public async Task<ItemResponseDto?> UpdateAsync(int p_id, UpdateItemRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        if (string.IsNullOrWhiteSpace(p_request.Name))
        {
            throw new ArgumentException(ErrorMessages.Item.NameRequired);
        }

        if (p_request.Price < 0)
        {
            throw new ArgumentException(ErrorMessages.Item.NegativePrice);
        }

        if (p_request.AlertQuantity < 0)
        {
            throw new ArgumentException(ErrorMessages.Item.NegativeAlertQuantity);
        }

        Item? item = await m_itemRepository.GetByIdForUpdateAsync(p_id);

        if (item is null)
        {
            return null;
        }

        item.Name = p_request.Name.Trim();
        item.Description = p_request.Description;
        item.Distributor = p_request.Distributor;
        item.Price = p_request.Price;
        item.AlertQuantity = p_request.AlertQuantity;
        if (p_request.IsActive.HasValue)
        {
            if (!p_request.IsActive.Value)
            {
                await m_itemRepository.RemoveInventoryLinesAsync(p_id);
            }

            item.IsActive = p_request.IsActive.Value;
        }
        item.LastUpdate = DateTime.UtcNow;

        if (item.Book is not null)
        {
            if (!string.IsNullOrWhiteSpace(p_request.Isbn))
            {
                item.Book.Isbn = p_request.Isbn.Trim();
            }

            if (p_request.PublicationDate.HasValue)
            {
                item.Book.PublicationDate = p_request.PublicationDate.Value;
            }

            if (p_request.CategoryIds is not null)
            {
                await m_itemRepository.LoadBookCategoriesAsync(item.Book);
                await UpdateBookCategoriesAsync(item.Book, p_request.CategoryIds);
            }

            if (p_request.Authors is not null)
            {
                List<int> authorIds = await m_bookRepository.ResolveAuthorIdsByNamesAsync(p_request.Authors);
                await m_itemRepository.LoadBookAuthorsAsync(item.Book);
                ReplaceAuthorBooks(item.Book, authorIds);
            }

            if (p_request.Publishers is not null)
            {
                List<int> publisherIds = await m_bookRepository.ResolvePublisherIdsByNamesAsync(p_request.Publishers);
                await m_itemRepository.LoadBookPublishersAsync(item.Book);
                ReplaceBookPublishers(item.Book, publisherIds);
            }
        }

        await m_itemRepository.SaveChangesAsync();

        Item? refreshedItem = await m_itemRepository.GetByIdReadOnlyAsync(p_id);

        if (refreshedItem is null)
        {
            return null;
        }

        return MapToResponseDto(refreshedItem);
    }

    private async Task UpdateBookCategoriesAsync(Book p_book, List<int> p_categoryIds)
    {
        List<int> categoryIds = p_categoryIds.Distinct().ToList();

        HashSet<int> existingCategoryIds = await m_categoryRepository.GetExistingActiveIdsAsync(categoryIds);
        List<int> missingCategoryIds = categoryIds.Where(p_id => !existingCategoryIds.Contains(p_id)).ToList();

        if (missingCategoryIds.Count > 0)
        {
            throw new InvalidOperationException(
                string.Format(ErrorMessages.Item.CategoriesNotFound, string.Join(", ", missingCategoryIds)));
        }

        p_book.BookCategories.Clear();

        foreach (int categoryId in categoryIds)
        {
            p_book.BookCategories.Add(new BookCategory
            {
                BookId = p_book.ItemId,
                CategoryId = categoryId
            });
        }
    }

    public async Task<ItemResponseDto?> UploadImageAsync(int p_id, Stream p_fileStream, string p_fileName)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        if (p_fileStream == null || p_fileStream.Length == 0)
        {
            throw new ArgumentException(ErrorMessages.Item.ImageFileRequired);
        }

        if (string.IsNullOrWhiteSpace(p_fileName))
        {
            throw new ArgumentException(ErrorMessages.Item.FileNameRequired);
        }

        string extension = Path.GetExtension(p_fileName);

        if (!AllowedImageExtensions.Contains(extension))
        {
            throw new ArgumentException(ErrorMessages.Item.InvalidImageFormat);
        }

        Item? item = await m_itemRepository.GetByIdAsync(p_id);

        if (item is null || !item.IsActive)
        {
            return null;
        }

        string uniqueFileName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
        string relativeWebPath = $"/{ItemsImageFolder}/{uniqueFileName}";

        string webRootPath = m_webHostEnvironment.WebRootPath
            ?? Path.Combine(m_webHostEnvironment.ContentRootPath, "wwwroot");

        string targetDirectory = Path.Combine(webRootPath, ItemsImageFolder);
        Directory.CreateDirectory(targetDirectory);

        string physicalPath = Path.Combine(targetDirectory, uniqueFileName);

        await using (FileStream fileStream = new(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await p_fileStream.CopyToAsync(fileStream);
        }

        item.ImageUrl = relativeWebPath;
        item.LastUpdate = DateTime.UtcNow;

        await m_itemRepository.SaveChangesAsync();

        return MapToResponseDto(item);
    }

    public async Task<bool> DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Item? item = await m_itemRepository.GetByIdAsync(p_id);

        if (item is null)
        {
            return false;
        }

        await m_itemRepository.RemoveInventoryLinesAsync(p_id);
        item.IsActive = false;
        item.LastUpdate = DateTime.UtcNow;

        await m_itemRepository.SaveChangesAsync();

        return true;
    }

    private static ItemResponseDto MapToResponseDto(Item p_item)
    {
        int totalQuantity = p_item.InventoryLines.Sum(p_line => p_line.Quantity);
        bool hasInventoryLines = p_item.InventoryLines.Count > 0;

        if (p_item.Book is null)
        {
            return new ItemResponseDto
            {
                Id = p_item.Id,
                Name = p_item.Name,
                Description = p_item.Description,
                Distributor = p_item.Distributor,
                ImageUrl = p_item.ImageUrl,
                Price = p_item.Price,
                AlertQuantity = p_item.AlertQuantity,
                TotalQuantity = totalQuantity,
                IsLowStock = hasInventoryLines && totalQuantity <= p_item.AlertQuantity,
                LastUpdate = p_item.LastUpdate,
                IsBook = false,
                IsActive = p_item.IsActive
            };
        }

        return MapToBookResponseDto(p_item, totalQuantity);
    }

    private static BookResponseDto MapToBookResponseDto(Item p_item, int p_totalQuantity)
    {
        Book book = p_item.Book!;

        return new BookResponseDto
        {
            Id = p_item.Id,
            Name = p_item.Name,
            Description = p_item.Description,
            Distributor = p_item.Distributor,
            ImageUrl = p_item.ImageUrl,
            Price = p_item.Price,
            AlertQuantity = p_item.AlertQuantity,
            TotalQuantity = p_totalQuantity,
            IsLowStock = p_item.InventoryLines.Count > 0 && p_totalQuantity <= p_item.AlertQuantity,
            LastUpdate = p_item.LastUpdate,
            IsBook = true,
            IsActive = p_item.IsActive,
            Isbn = book.Isbn,
            PublicationDate = book.PublicationDate,
            Authors = MapAuthorNames(p_item),
            AuthorIds = MapAuthorIds(p_item),
            Publishers = MapPublisherNames(p_item),
            CategoryIds = MapCategoryIds(p_item),
            Categories = MapCategoryNames(p_item)
        };
    }

    private async Task<List<int>> ResolveAuthorIdsAsync(
        IReadOnlyCollection<int> p_authorIds,
        IReadOnlyCollection<string> p_authorNames)
    {
        List<int> resolvedIds = p_authorIds.Distinct().ToList();
        List<int> resolvedByName = await m_bookRepository.ResolveAuthorIdsByNamesAsync(p_authorNames);

        return resolvedIds
            .Concat(resolvedByName)
            .Distinct()
            .ToList();
    }

    private async Task<List<int>> ResolvePublisherIdsAsync(
        IReadOnlyCollection<int> p_publisherIds,
        IReadOnlyCollection<string> p_publisherNames)
    {
        List<int> resolvedIds = p_publisherIds.Distinct().ToList();
        List<int> resolvedByName = await m_bookRepository.ResolvePublisherIdsByNamesAsync(p_publisherNames);

        return resolvedIds
            .Concat(resolvedByName)
            .Distinct()
            .ToList();
    }

    private static void ReplaceAuthorBooks(Book p_book, IReadOnlyCollection<int> p_authorIds)
    {
        p_book.AuthorBooks.Clear();

        foreach (int authorId in p_authorIds)
        {
            p_book.AuthorBooks.Add(new AuthorBook
            {
                BookId = p_book.ItemId,
                AuthorId = authorId
            });
        }
    }

    private static void ReplaceBookPublishers(Book p_book, IReadOnlyCollection<int> p_publisherIds)
    {
        p_book.BookPublishers.Clear();

        foreach (int publisherId in p_publisherIds)
        {
            p_book.BookPublishers.Add(new BookPublisher
            {
                BookId = p_book.ItemId,
                PublisherId = publisherId
            });
        }
    }

    private static List<string> MapPublisherNames(Item p_item)
    {
        if (p_item.Book?.BookPublishers is not { Count: > 0 } bookPublishers)
        {
            return new List<string>();
        }

        return bookPublishers
            .Select(p_bookPublisher => p_bookPublisher.Publisher?.Name)
            .Where(p_name => !string.IsNullOrWhiteSpace(p_name))
            .Select(p_name => p_name!)
            .Distinct()
            .ToList();
    }

    private static List<int> MapCategoryIds(Item p_item)
    {
        if (p_item.Book?.BookCategories is not { Count: > 0 } bookCategories)
        {
            return new List<int>();
        }

        List<int> categoryIds = bookCategories
            .Select(p_bookCategory => p_bookCategory.CategoryId)
            .Distinct()
            .ToList();

        return categoryIds;
    }

    private static List<string> MapAuthorNames(Item p_item)
    {
        if (p_item.Book?.AuthorBooks is not { Count: > 0 } authorBooks)
        {
            return new List<string>();
        }

        return authorBooks
            .Select(p_authorBook => p_authorBook.Author?.Name)
            .Where(p_name => !string.IsNullOrWhiteSpace(p_name))
            .Select(p_name => p_name!)
            .Distinct()
            .ToList();
    }

    private static List<int> MapAuthorIds(Item p_item)
    {
        if (p_item.Book?.AuthorBooks is not { Count: > 0 } authorBooks)
        {
            return new List<int>();
        }

        return authorBooks
            .Select(p_authorBook => p_authorBook.AuthorId)
            .Distinct()
            .ToList();
    }

    private static List<string> MapCategoryNames(Item p_item)
    {
        if (p_item.Book?.BookCategories is not { Count: > 0 } bookCategories)
        {
            return new List<string>();
        }

        return bookCategories
            .Select(p_bookCategory => p_bookCategory.Category?.Name)
            .Where(p_name => !string.IsNullOrWhiteSpace(p_name))
            .Select(p_name => p_name!)
            .Distinct()
            .ToList();
    }
}
