using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository m_authorRepository;

    public AuthorService(IAuthorRepository p_authorRepository)
    {
        m_authorRepository = p_authorRepository;
    }

    public async Task<IEnumerable<AuthorResponseDto>> GetAllAsync()
    {
        IEnumerable<Author> authors = await m_authorRepository.GetAllAsync();
        return authors.Select(MapToDto);
    }

    public async Task<AuthorResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Author? author = await m_authorRepository.GetByIdAsync(p_id);
        return author is null ? null : MapToDto(author);
    }

    public async Task<AuthorResponseDto> CreateAsync(CreateAuthorRequest p_request)
    {
        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        Author author = new Author
        {
            Name = normalizedName
        };

        await m_authorRepository.AddAsync(author);
        await m_authorRepository.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task<AuthorResponseDto> UpdateAsync(int p_id, UpdateAuthorRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        Author? existingAuthor = await m_authorRepository.GetByIdAsync(p_id);
        if (existingAuthor is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Author.NotFound);
        }

        existingAuthor.Name = normalizedName;
        m_authorRepository.Update(existingAuthor);
        await m_authorRepository.SaveChangesAsync();

        return MapToDto(existingAuthor);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Author? existingAuthor = await m_authorRepository.GetByIdAsync(p_id);
        if (existingAuthor is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Author.NotFound);
        }

        m_authorRepository.Delete(existingAuthor);
        await m_authorRepository.SaveChangesAsync();
    }

    private static AuthorResponseDto MapToDto(Author p_author)
    {
        return new AuthorResponseDto
        {
            Id = p_author.Id,
            Name = p_author.Name
        };
    }

    private static string NormalizeName(string p_name)
    {
        return p_name.Trim();
    }

    private static void ValidateName(string p_name)
    {
        if (string.IsNullOrWhiteSpace(p_name))
        {
            throw new ArgumentException(ErrorMessages.Author.NameRequired);
        }

        if (p_name.Length > 200)
        {
            throw new ArgumentException(ErrorMessages.Author.NameTooLong);
        }
    }
}
