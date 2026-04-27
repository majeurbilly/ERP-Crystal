using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly CrystalDbContext m_context;

        public LocationRepository(CrystalDbContext p_context)
        {
            m_context = p_context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await m_context.Locations.ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int p_id)
        {
            return await m_context.Locations.FirstOrDefaultAsync(l => l.Id == p_id);
        }

        public async Task<Location?> GetByTitleAsync(string p_title)
        {
            return await m_context.Locations.FirstOrDefaultAsync(l => l.Title == p_title);
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

        public async Task SaveChangesAsync()
        {
            await m_context.SaveChangesAsync();
        }
    }
}