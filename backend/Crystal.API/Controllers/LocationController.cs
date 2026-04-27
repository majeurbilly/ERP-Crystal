using Crystal.Core.DTOs.Responses;
using Crystal.API.DTOs.Location;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService m_locationService;

        public LocationsController(ILocationService p_locationService)
        {
            m_locationService = p_locationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetAll()
        {
            IEnumerable<Location> locations = await m_locationService.GetAllAsync();

            IEnumerable<LocationDto> result = locations.Select(location => new LocationDto
            {
                Id = location.Id,
                Title = location.Title,
                Address = location.Address,
                Description = location.Description
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LocationDto>> GetById(int p_id)
        {
            Location? location = await m_locationService.GetByIdAsync(p_id);
            if (location == null)
            {
                return NotFound();
            }

            LocationDto result = new()
            {
                Id = location.Id,
                Title = location.Title,
                Address = location.Address,
                Description = location.Description
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Gerant")]
        public async Task<ActionResult<LocationDto>> Create([FromBody] CreateLocationDto p_dto)
        {
            Location location = new()
            {
                Title = p_dto.Title,
                Address = p_dto.Address,
                Description = p_dto.Description
            };

            Location created = await m_locationService.CreateAsync(location);

            LocationDto result = new()
            {
                Id = created.Id,
                Title = created.Title,
                Address = created.Address,
                Description = created.Description
            };

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Gerant")]
        public async Task<ActionResult<LocationDto>> Update(int p_id, [FromBody] UpdateLocationDto p_dto)
        {
            Location location = new()
            {
                Title = p_dto.Title,
                Address = p_dto.Address,
                Description = p_dto.Description
            };

            Location updated = await m_locationService.UpdateAsync(p_id, location);

            LocationDto result = new()
            {
                Id = updated.Id,
                Title = updated.Title,
                Address = updated.Address,
                Description = updated.Description
            };

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int p_id)
        {
            await m_locationService.DeleteAsync(p_id);
            return NoContent();
        }
    }
}