using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly IBookRepository m_bookRepository;

    public BookService(IBookRepository p_bookRepository)
    {
        m_bookRepository = p_bookRepository;
    }

    public async Task<BookResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Book? book = await m_bookRepository.GetByIdAsync(p_id);

        if (book is null)
        {
            return null;
        }

        return MapToDto(book);
    }

    public async Task<BookResponseDto?> UpdateBookRelationsAsync(int p_id, UpdateBookRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Book? book = await m_bookRepository.GetByIdForUpdateAsync(p_id);

        if (book is null)
        {
            return null;
        }

        List<int> authorIds = p_request.AuthorIds.Distinct().ToList();
        List<int> categoryIds = p_request.CategoryIds.Distinct().ToList();
        List<int> publisherIds = p_request.PublisherIds.Distinct().ToList();

        await ValidateRelationIdsAsync(authorIds, categoryIds, publisherIds);

        if (!string.IsNullOrWhiteSpace(p_request.Isbn))
        {
            book.Isbn = p_request.Isbn.Trim();
        }

        if (p_request.PublicationDate.HasValue)
        {
            book.PublicationDate = p_request.PublicationDate.Value;
        }

        ReplaceAuthorBooks(book, authorIds);
        ReplaceBookCategories(book, categoryIds);
        ReplaceBookPublishers(book, publisherIds);

        await m_bookRepository.SaveChangesAsync();

        Book? updatedBook = await m_bookRepository.GetByIdAsync(p_id);

        if (updatedBook is null)
        {
            return null;
        }

        return MapToDto(updatedBook);
    }

    private async Task ValidateRelationIdsAsync(
        IReadOnlyCollection<int> p_authorIds,
        IReadOnlyCollection<int> p_categoryIds,
        IReadOnlyCollection<int> p_publisherIds)
    {
        HashSet<int> existingAuthorIds = await m_bookRepository.GetExistingAuthorIdsAsync(p_authorIds);
        List<int> missingAuthorIds = p_authorIds.Where(p_id => !existingAuthorIds.Contains(p_id)).ToList();

        if (missingAuthorIds.Count > 0)
        {
            throw new InvalidOperationException(
                string.Format(ErrorMessages.Book.AuthorsNotFound, string.Join(", ", missingAuthorIds)));
        }

        HashSet<int> existingCategoryIds = await m_bookRepository.GetExistingCategoryIdsAsync(p_categoryIds);
        List<int> missingCategoryIds = p_categoryIds.Where(p_id => !existingCategoryIds.Contains(p_id)).ToList();

        if (missingCategoryIds.Count > 0)
        {
            throw new InvalidOperationException(
                string.Format(ErrorMessages.Book.CategoriesNotFound, string.Join(", ", missingCategoryIds)));
        }

        HashSet<int> existingPublisherIds = await m_bookRepository.GetExistingPublisherIdsAsync(p_publisherIds);
        List<int> missingPublisherIds = p_publisherIds.Where(p_id => !existingPublisherIds.Contains(p_id)).ToList();

        if (missingPublisherIds.Count > 0)
        {
            throw new InvalidOperationException(
                string.Format(ErrorMessages.Book.PublishersNotFound, string.Join(", ", missingPublisherIds)));
        }
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

    private static void ReplaceBookCategories(Book p_book, IReadOnlyCollection<int> p_categoryIds)
    {
        p_book.BookCategories.Clear();

        foreach (int categoryId in p_categoryIds)
        {
            p_book.BookCategories.Add(new BookCategory
            {
                BookId = p_book.ItemId,
                CategoryId = categoryId
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

    private static BookResponseDto MapToDto(Book p_book)
    {
        int totalQuantity = p_book.Item.InventoryLines.Sum(p_inventoryLine => p_inventoryLine.Quantity);

        return new BookResponseDto
        {
            Id = p_book.Item.Id,
            Name = p_book.Item.Name,
            Description = p_book.Item.Description,
            Distributor = p_book.Item.Distributor,
            ImageUrl = p_book.Item.ImageUrl,
            Price = p_book.Item.Price,
            TotalQuantity = totalQuantity,
            AlertQuantity = p_book.Item.AlertQuantity,
            IsLowStock = totalQuantity <= p_book.Item.AlertQuantity,
            LastUpdate = p_book.Item.LastUpdate,
            IsBook = true,
            IsActive = p_book.Item.IsActive,

            Authors = p_book.AuthorBooks
                .Select(p_authorBook => p_authorBook.Author.Name)
                .ToList(),

            Categories = p_book.BookCategories
                .Select(p_bookCategory => p_bookCategory.Category.Name)
                .ToList(),

            CategoryIds = p_book.BookCategories
                .Select(p_bookCategory => p_bookCategory.CategoryId)
                .Distinct()
                .ToList(),

            Isbn = p_book.Isbn,
            PublicationDate = p_book.PublicationDate,

            Publishers = p_book.BookPublishers
                .Select(p_bookPublisher => p_bookPublisher.Publisher.Name)
                .ToList()
        };
    }
}
