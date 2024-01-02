using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Controllers;

[Route("tables/[controller]")]
[ExcludeFromCodeCoverage]
public class TodoItemController : TableController<TodoItem>
{
    public TodoItemController(IRepository<TodoItem> repository) : base(repository)
    {
    }
}
