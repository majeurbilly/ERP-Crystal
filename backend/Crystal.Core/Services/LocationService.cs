using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;

namespace Crystal.Core.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository m_locationRepository;

        public LocationService(ILocationRepository p_locationRepository)
        {
            m_locationRepository = p_locationRepository;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await m_locationRepository.GetAllAsync();
        }

        public async Task<Location?> GetByIdAsync(int p_id)
        {
            if (p_id <= 0)
                throw new ArgumentException("The identifier is invalid.");

            return await m_locationRepository.GetByIdAsync(p_id);
        }

        public async Task<Location> CreateAsync(Location p_location)
        {
            ValidateLocation(p_location);

            Location? existingLocation = await m_locationRepository.GetByTitleAsync(p_location.Title);
            if (existingLocation != null)
                throw new InvalidOperationException("A location with this title already exists.");

            await m_locationRepository.AddAsync(p_location);
            await m_locationRepository.SaveChangesAsync();

            return p_location;
        }

        public async Task<Location> UpdateAsync(int p_id, Location p_updatedLocation)
        {
            if (p_id <= 0)
                throw new ArgumentException("The identifier is invalid.");

            ValidateLocation(p_updatedLocation);

            Location? existingLocation = await m_locationRepository.GetByIdAsync(p_id);
            if (existingLocation == null)
                throw new KeyNotFoundException("Location not found.");

            Location? locationWithSameTitle = await m_locationRepository.GetByTitleAsync(p_updatedLocation.Title);
            if (locationWithSameTitle != null && locationWithSameTitle.Id != p_id)
                throw new InvalidOperationException("Another location with this title already exists.");

            existingLocation.Title = p_updatedLocation.Title;
            existingLocation.Address = p_updatedLocation.Address;
            existingLocation.Description = p_updatedLocation.Description;

            m_locationRepository.Update(existingLocation);
            await m_locationRepository.SaveChangesAsync();

            return existingLocation;
        }

        public async Task DeleteAsync(int p_id)
        {
            if (p_id <= 0)
                throw new ArgumentException("The identifier is invalid.");

            Location? existingLocation = await m_locationRepository.GetByIdAsync(p_id);
            if (existingLocation == null)
                throw new KeyNotFoundException("Location not found.");

            m_locationRepository.Delete(existingLocation);
            await m_locationRepository.SaveChangesAsync();
        }

        private static void ValidateLocation(Location p_location)
        {
            if (p_location == null)
                throw new ArgumentNullException(nameof(p_location));

            if (string.IsNullOrWhiteSpace(p_location.Title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(p_location.Address))
                throw new ArgumentException("Address is required.");

            if (p_location.Title.Length > 100)
                throw new ArgumentException("Title is too long.");

            if (p_location.Address.Length > 200)
                throw new ArgumentException("Address is too long.");

            if (!string.IsNullOrWhiteSpace(p_location.Description) && p_location.Description.Length > 500)
                throw new ArgumentException("Description is too long.");
        }
    }
}