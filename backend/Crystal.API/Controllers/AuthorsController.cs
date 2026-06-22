using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/authors")]
[Authorize]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService m_authorService;

    public AuthorsController(IAuthorService p_authorService)
    {
        m_authorService = p_authorService;
    }

    [HttpGet]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Author)]
    public async Task<ActionResult<IEnumerable<AuthorResponseDto>>> GetAll()
    {
        IEnumerable<AuthorResponseDto> authors = await m_authorService.GetAllAsync();
        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Author)]
    public async Task<ActionResult<AuthorResponseDto>> GetById([FromRoute(Name = "id")] int p_id)
    {
        AuthorResponseDto? author = await m_authorService.GetByIdAsync(p_id);

        if (author is null)
        {
            return NotFound();
        }

        return Ok(author);
    }

    [HttpPost]
    [RequirePermission(PermissionActions.Create, PermissionSubjects.Author)]
    public async Task<ActionResult<AuthorResponseDto>> Create([FromBody] CreateAuthorRequest p_request)
    {
        AuthorResponseDto created = await m_authorService.CreateAsync(p_request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.Author)]
    public async Task<ActionResult<AuthorResponseDto>> Update(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateAuthorRequest p_request)
    {
        AuthorResponseDto updated = await m_authorService.UpdateAsync(p_id, p_request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(PermissionActions.Delete, PermissionSubjects.Author)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] int p_id)
    {
        await m_authorService.DeleteAsync(p_id);
        return NoContent();
    }
}
