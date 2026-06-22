using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]
public class BookController : ControllerBase
{
    private readonly IBookService m_bookService;

    public BookController(IBookService p_bookService)
    {
        m_bookService = p_bookService;
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Item)]
    public async Task<IActionResult> GetBookById([FromRoute(Name = "id")] int p_id)
    {
        BookResponseDto? book = await m_bookService.GetByIdAsync(p_id);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionActions.Update, PermissionSubjects.Item)]
    public async Task<IActionResult> UpdateBook(
        [FromRoute(Name = "id")] int p_id,
        [FromBody] UpdateBookRequest p_request)
    {
        BookResponseDto? book = await m_bookService.UpdateBookRelationsAsync(p_id, p_request);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(book);
    }
}
