using Microsoft.AspNetCore.Mvc;
using api.Db;

namespace api.Controllers;

[Route("fix-index")]
public class FixIndexController : Controller
{
    private readonly TodoAppContext _context;

    public FixIndexController(TodoAppContext context) 
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> FixIndexAsync()
    {
        await _context.FixIndex();
        return NoContent();
    }
}