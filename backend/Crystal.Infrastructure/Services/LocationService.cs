using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository m_locationRepository;
    private readonly IInventoryRepository m_inventoryRepository;

    public LocationService(
        ILocationRepository p_locationRepository,
        IInventoryRepository p_inventoryRepository)
    {
        m_locationRepository = p_locationRepository;
        m_inventoryRepository = p_inventoryRepository;
    }

    public async Task<IEnumerable<LocationResponseDto>> GetAllAsync()
    {
        return await m_locationRepository.GetAllAsync();
    }

    public async Task<List<LocationOptionResponseDto>> GetDropdownOptionsAsync()
    {
        return await m_locationRepository.GetDropdownOptionsAsync();
    }

    public async Task<LocationResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Location? location = await m_locationRepository.GetByIdAsync(p_id);

        if (location is null)
        {
            return null;
        }

        return MapToDto(location);
    }

    public async Task<LocationResponseDto> CreateAsync(CreateLocationRequestDto p_request)
    {
        string normalizedTitle = NormalizeTitle(p_request.Title);
        string normalizedAddress = NormalizeRequiredText(p_request.Address);
        string normalizedDescription = NormalizeDescription(p_request.Description);

        Location location = new Location
        {
            Title = normalizedTitle,
            Address = normalizedAddress,
            Description = normalizedDescription
        };

        ValidateLocation(location);

        Location? existingLocation = await m_locationRepository.GetByTitleAsync(normalizedTitle);
        if (existingLocation is not null)
        {
            throw new InvalidOperationException(ErrorMessages.Location.TitleAlreadyExists);
        }

        await m_locationRepository.AddAsync(location);
        await m_locationRepository.SaveChangesAsync();

        return MapToDto(location);
    }

    public async Task<LocationResponseDto> UpdateAsync(int p_id, UpdateLocationRequestDto p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        string normalizedTitle = NormalizeTitle(p_request.Title);
        string normalizedAddress = NormalizeRequiredText(p_request.Address);
        string normalizedDescription = NormalizeDescription(p_request.Description);

        Location updatedLocation = new Location
        {
            Title = normalizedTitle,
            Address = normalizedAddress,
            Description = normalizedDescription
        };

        ValidateLocation(updatedLocation);

        Location? existingLocation = await m_locationRepository.GetByIdAsync(p_id);
        if (existingLocation is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Location.NotFound);
        }

        Location? locationWithSameTitle = await m_locationRepository.GetByTitleAsync(normalizedTitle);
        if (locationWithSameTitle is not null && locationWithSameTitle.Id != p_id)
        {
            throw new InvalidOperationException(ErrorMessages.Location.TitleAlreadyExists);
        }

        existingLocation.Title = updatedLocation.Title;
        existingLocation.Address = updatedLocation.Address;
        existingLocation.Description = updatedLocation.Description;

        m_locationRepository.Update(existingLocation);
        await m_locationRepository.SaveChangesAsync();

        return MapToDto(existingLocation);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Location? existingLocation = await m_locationRepository.GetByIdAsync(p_id);
        if (existingLocation is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Location.NotFound);
        }

        bool hasInventory = await m_inventoryRepository.HasInventoryForLocationAsync(p_id);
        if (hasInventory)
        {
            throw new InvalidOperationException(ErrorMessages.Location.HasInventoryCannotDelete);
        }

        m_locationRepository.Delete(existingLocation);
        await m_locationRepository.SaveChangesAsync();
    }

    private static LocationResponseDto MapToDto(Location p_location)
    {
        return new LocationResponseDto
        {
            Id = p_location.Id,
            Title = p_location.Title,
            Address = p_location.Address,
            Description = p_location.Description
        };
    }

    private static string NormalizeTitle(string p_title)
    {
        return p_title.Trim();
    }

    private static string NormalizeRequiredText(string p_value)
    {
        return p_value.Trim();
    }

    private static string NormalizeDescription(string p_description)
    {
        return string.IsNullOrWhiteSpace(p_description) ? string.Empty : p_description.Trim();
    }

    private static void ValidateLocation(Location p_location)
    {
        if (p_location is null)
        {
            throw new ArgumentNullException(nameof(p_location));
        }

        if (string.IsNullOrWhiteSpace(p_location.Title))
        {
            throw new ArgumentException(ErrorMessages.Location.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(p_location.Address))
        {
            throw new ArgumentException(ErrorMessages.Location.AddressRequired);
        }

        if (p_location.Title.Length > 100)
        {
            throw new ArgumentException(ErrorMessages.Location.TitleTooLong);
        }

        if (p_location.Address.Length > 200)
        {
            throw new ArgumentException(ErrorMessages.Location.AddressTooLong);
        }

        if (p_location.Description.Length > 500)
        {
            throw new ArgumentException(ErrorMessages.Location.DescriptionTooLong);
        }
    }
}
