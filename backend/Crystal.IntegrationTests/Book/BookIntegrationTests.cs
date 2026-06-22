using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Book;

public sealed class BookIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public BookIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task UpdateBookRelations_OverwritesAuthorsAndCategoriesInDatabase()
    {
        await AuthenticateAsGerantAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.AuthorBooks.RemoveRange(context.AuthorBooks);
        context.BookCategories.RemoveRange(context.BookCategories);
        context.BookPublishers.RemoveRange(context.BookPublishers);
        context.Books.RemoveRange(context.Books);
        context.Items.RemoveRange(context.Items);
        context.Authors.RemoveRange(context.Authors);
        context.Categories.RemoveRange(context.Categories);
        context.Publishers.RemoveRange(context.Publishers);
        await context.SaveChangesAsync();

        Author authorInitial = new() { Name = "Auteur initial" };
        Author authorReplacement = new() { Name = "Auteur remplacement" };
        Crystal.Core.Entities.Category categoryInitial = new() { Name = "Initial Category", IsDeleted = false };
        Crystal.Core.Entities.Category categoryReplacement = new() { Name = "Replacement Category", IsDeleted = false };
        Publisher publisherInitial = new() { Name = "Éditeur initial" };

        context.Authors.AddRange(authorInitial, authorReplacement);
        context.Categories.AddRange(categoryInitial, categoryReplacement);
        context.Publishers.Add(publisherInitial);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Livre test M2M",
            Description = "Test relations",
            Price = 29.99m,
            AlertQuantity = 2,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Crystal.Core.Entities.Book
            {
                PublicationDate = new DateOnly(2020, 1, 1),
                AuthorBooks =
                [
                    new AuthorBook { AuthorId = authorInitial.Id }
                ],
                BookCategories =
                [
                    new BookCategory { CategoryId = categoryInitial.Id }
                ],
                BookPublishers =
                [
                    new BookPublisher { PublisherId = publisherInitial.Id }
                ]
            }
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        int bookId = item.Id;

        UpdateBookRequest updateRequest = new()
        {
            AuthorIds = [authorReplacement.Id],
            CategoryIds = [categoryReplacement.Id],
            PublisherIds = []
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            $"/api/books/{bookId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        BookResponseDto? body = await response.Content.ReadFromJsonAsync<BookResponseDto>();
        Assert.NotNull(body);
        Assert.Single(body.Authors);
        Assert.Equal("Auteur remplacement", body.Authors[0]);
        Assert.Single(body.Categories);
        Assert.Equal("Replacement Category", body.Categories[0]);
        Assert.Empty(body.Publishers);

        context.ChangeTracker.Clear();

        List<AuthorBook> authorBooks = await context.AuthorBooks
            .Where(p_ab => p_ab.BookId == bookId)
            .ToListAsync();

        List<BookCategory> bookCategories = await context.BookCategories
            .Where(p_bc => p_bc.BookId == bookId)
            .ToListAsync();

        List<BookPublisher> bookPublishers = await context.BookPublishers
            .Where(p_bp => p_bp.BookId == bookId)
            .ToListAsync();

        Assert.Single(authorBooks);
        Assert.Equal(authorReplacement.Id, authorBooks[0].AuthorId);

        Assert.Single(bookCategories);
        Assert.Equal(categoryReplacement.Id, bookCategories[0].CategoryId);

        Assert.Empty(bookPublishers);
    }

    [Fact]
    public async Task UpdateBookRelations_Returns409_WhenAuthorIdDoesNotExist()
    {
        await AuthenticateAsGerantAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        context.Books.RemoveRange(context.Books);
        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();

        Item item = new()
        {
            Name = "Livre sans auteur valide",
            Price = 10m,
            AlertQuantity = 1,
            LastUpdate = DateTime.UtcNow,
            IsActive = true,
            Book = new Crystal.Core.Entities.Book
            {
                PublicationDate = new DateOnly(2021, 5, 5)
            }
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        UpdateBookRequest updateRequest = new()
        {
            AuthorIds = [999999],
            CategoryIds = [],
            PublisherIds = []
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            $"/api/books/{item.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBookRelations_Returns403_WhenEmployeeAuthenticated()
    {
        await AuthenticateAsEmployeeAsync();

        UpdateBookRequest updateRequest = new()
        {
            AuthorIds = [],
            CategoryIds = [],
            PublisherIds = []
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync(
            "/api/books/1",
            updateRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
