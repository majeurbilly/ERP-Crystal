using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;

namespace Crystal.Core.Interfaces.Repositories
{
    public interface ILocationRepository
    {
        Task<List<LocationResponseDto>> GetAllAsync();

        Task<List<LocationOptionResponseDto>> GetDropdownOptionsAsync();

        Task<Location?> GetByIdAsync(int p_id);
        Task<Location?> GetByTitleAsync(string p_title);
        Task AddAsync(Location p_location);
        void Update(Location p_location);
        void Delete(Location p_location);
        Task SaveChangesAsync();
    }
}
