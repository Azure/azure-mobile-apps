using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using api.Db;

namespace api.Controllers;

[Route("tables/todoitem")]
public class TodoItemController : TableController<TodoItem>
{
    public TodoItemController(TodoAppContext context) : base(new EntityTableRepository<TodoItem>(context))
    {
        
    }
}