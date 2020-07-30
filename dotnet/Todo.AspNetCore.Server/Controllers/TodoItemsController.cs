using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Microsoft.AspNetCore.Mvc;
using Todo.AspNetCore.Server.Database;

namespace Todo.AspNetCore.Server.Controllers
{
    /// <summary>
    /// API Controller for the /tables/todoitems endpoint, which is an Azure Mobile Apps
    /// synchronizable table controller.
    /// </summary>
    [Route("tables/todoitems")]
    [ApiController]
    public class TodoItemsController : TableController<TodoItemDTO>
    {
        public TodoItemsController(AppDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<TodoItemDTO>
            {
                DataView = item => item.OwnerId == HttpContext.User.Identity.Name,
                MaxTop = 1000,
                SoftDeleteEnabled = false
            };
            TableRepository = new EntityTableRepository<TodoItemDTO>(context);
        }

        /// <summary>
        /// User is authorized for the operation if it's a list (controlled by the DataView above)
        /// or a new record or the OwnerId matches the user id.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="item">The item being resolved</param>
        /// <returns></returns>
        public override bool IsAuthorized(TableOperation operation, TodoItemDTO item)
            => operation == TableOperation.List || operation == TableOperation.Create || item.OwnerId == HttpContext.User.Identity.Name;

        /// <summary>
        /// When we store data, we always tag the record with the users ownerId
        /// </summary>
        /// <param name="item">The item being stored</param>
        /// <returns>The item to be stored</returns>
        public override TodoItemDTO PrepareItemForStore(TodoItemDTO item)
        {
            item.OwnerId = HttpContext.User.Identity.Name;
            return base.PrepareItemForStore(item);
        }
    }
}
