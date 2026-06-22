using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories;

public class LocationRepository : RepositoryBase, ILocationRepository
{
    public LocationRepository(CrystalDbContext p_context)
        : base(p_context)
    {
    }

    public async Task<List<LocationResponseDto>> GetAllAsync()
    {
        return await m_context.Locations
            .AsNoTracking()
            .OrderBy(p_location => p_location.Title)
            .Select(p_location => new LocationResponseDto
            {
                Id = p_location.Id,
                Title = p_location.Title,
                Address = p_location.Address,
                Description = p_location.Description
            })
            .ToListAsync();
    }

    public async Task<List<LocationOptionResponseDto>> GetDropdownOptionsAsync()
    {
        return await m_context.Locations
            .AsNoTracking()
            .OrderBy(p_location => p_location.Title)
            .Select(p_location => new LocationOptionResponseDto
            {
                Id = p_location.Id,
                Title = p_location.Title
            })
            .ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(int p_id)
    {
        return await m_context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(p_location => p_location.Id == p_id);
    }

    public async Task<Location?> GetByTitleAsync(string p_title)
    {
        return await m_context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(p_location => p_location.Title == p_title);
    }

    public async Task AddAsync(Location p_location)
    {
        await m_context.Locations.AddAsync(p_location);
    }

    public void Update(Location p_location)
    {
        m_context.Locations.Update(p_location);
    }

    public void Delete(Location p_location)
    {
        m_context.Locations.Remove(p_location);
    }

}
