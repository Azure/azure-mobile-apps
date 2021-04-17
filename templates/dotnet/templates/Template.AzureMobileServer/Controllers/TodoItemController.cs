using Template.AzureMobileServer.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AzureMobile.Server;
using Microsoft.AzureMobile.Server.EFCore;

namespace Template.AzureMobileServer.Controllers
{
    [Route("tables/[controller]")]
    public class TodoItemController : TableController<TodoItem>
    {
        public TodoItemController(AppDbContext context) : base()
        {
            Repository = new EntityTableRepository<TodoItem>(context);
        }
    }
}