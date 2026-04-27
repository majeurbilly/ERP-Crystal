using Crystal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Core.Interfaces.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int p_id);
        Task<Location> CreateAsync(Location p_location);
        Task<Location> UpdateAsync(int p_id, Location p_updatedLocation);
        Task DeleteAsync(int p_id);
    }
}
