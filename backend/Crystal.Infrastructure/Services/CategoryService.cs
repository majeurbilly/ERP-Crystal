using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository m_categoryRepository;

    public CategoryService(ICategoryRepository p_categoryRepository)
    {
        m_categoryRepository = p_categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        IEnumerable<Category> categories = await m_categoryRepository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Category? category = await m_categoryRepository.GetByIdAsync(p_id);

        if (category is null)
        {
            return null;
        }

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto p_request)
    {
        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        Category? existingCategory = await m_categoryRepository.GetByNameAsync(normalizedName);
        if (existingCategory is not null)
        {
            throw new InvalidOperationException(ErrorMessages.Category.NameAlreadyExists);
        }

        Category category = new Category
        {
            Name = normalizedName,
            IsDeleted = false
        };

        await m_categoryRepository.AddAsync(category);
        await m_categoryRepository.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(int p_id, UpdateCategoryRequestDto p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        Category? existingCategory = await m_categoryRepository.GetByIdAsync(p_id);
        if (existingCategory is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Category.NotFound);
        }

        Category? categoryWithSameName = await m_categoryRepository.GetByNameAsync(normalizedName);
        if (categoryWithSameName is not null && categoryWithSameName.Id != p_id)
        {
            throw new InvalidOperationException(ErrorMessages.Category.NameAlreadyExists);
        }

        existingCategory.Name = normalizedName;
        m_categoryRepository.Update(existingCategory);
        await m_categoryRepository.SaveChangesAsync();

        return MapToDto(existingCategory);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Category? existingCategory = await m_categoryRepository.GetByIdAsync(p_id);
        if (existingCategory is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Category.NotFound);
        }

        m_categoryRepository.SoftDelete(existingCategory);
        await m_categoryRepository.SaveChangesAsync();
    }

    private static CategoryResponseDto MapToDto(Category p_category)
    {
        return new CategoryResponseDto
        {
            Id = p_category.Id,
            Name = p_category.Name
        };
    }

    private static string NormalizeName(string p_name)
    {
        return p_name.Trim();
    }

    private static void ValidateName(string p_normalizedName)
    {
        if (string.IsNullOrWhiteSpace(p_normalizedName))
        {
            throw new ArgumentException(ErrorMessages.Category.NameRequired);
        }

        if (p_normalizedName.Length > 100)
        {
            throw new ArgumentException(ErrorMessages.Category.NameTooLong);
        }
    }
}
