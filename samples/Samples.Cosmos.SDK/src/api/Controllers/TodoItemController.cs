using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;
using api.Db;
using Microsoft.AspNetCore.Datasync.CosmosDb;

namespace api.Controllers;

[Route("tables/todoitem")]
public class TodoItemController : TableController<TodoItem>
{
    public TodoItemController(CosmosRepository<TodoItem> repository) : base(repository)
    {
        Options = new TableControllerOptions { PageSize = 5 };
    }
}