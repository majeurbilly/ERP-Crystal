using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService m_categoryService;

    public CategoriesController(ICategoryService p_categoryService)
    {
        m_categoryService = p_categoryService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Category)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        IEnumerable<CategoryResponseDto> categories = await m_categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Category)]
    public async Task<ActionResult<CategoryResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        CategoryResponseDto? category = await m_categoryService.GetByIdAsync(p_id);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.Category)]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryRequestDto p_request)
    {
        CategoryResponseDto created = await m_categoryService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.Category)]
    public async Task<ActionResult<CategoryResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateCategoryRequestDto p_request)
    {
        CategoryResponseDto updated = await m_categoryService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.Category)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_categoryService.DeleteAsync(p_id);
        return NoContent();
    }
}
