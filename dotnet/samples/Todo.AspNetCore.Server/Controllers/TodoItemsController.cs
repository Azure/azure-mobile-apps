// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using Microsoft.Zumo.Server;

using Microsoft.Zumo.Server.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Todo.AspNetCore.Server.Database;
using Microsoft.Zumo.Server;
using System.Linq;

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
            TableControllerOptions = new TableControllerOptions
            {
                SoftDeleteEnabled = false
            };
            TableRepository = new EntityTableRepository<TodoItemDTO>(context);
        }

        [HttpGet, ZumoQuery(MaxTop = 1000)]
        public override IActionResult GetItems() 
            => Ok(TableRepository.AsQueryable().Where(item => item.OwnerId == CurrentUserId));

        /// <summary>
        /// User is authorized for the operation if it's a list (controlled by the DataView above)
        /// or a new record or the OwnerId matches the user id.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="item">The item being resolved</param>
        /// <returns></returns>
        public override bool IsAuthorized(TableOperation operation, TodoItemDTO item)
            => operation == TableOperation.Create || item.OwnerId == CurrentUserId;

        /// <summary>
        /// When we store data, we always tag the record with the users ownerId
        /// </summary>
        /// <param name="item">The item being stored</param>
        /// <returns>The item to be stored</returns>
        public override TodoItemDTO PrepareItemForStore(TodoItemDTO item)
        {
            item.OwnerId = CurrentUserId;
            return base.PrepareItemForStore(item);
        }

        /// <summary>
        /// Returns the UserId.  We use ObjectId for this, since the ObjectId is stable and unique to the user
        /// across multiple applications.  Two different applications signing in as the same user will receive
        /// the same ObjectId.  The .Identity.Name (NameIdentifierId) returns the sub claim that is unique to
        /// a particular application ID.
        /// </summary>
        private string CurrentUserId
        {
            get => HttpContext.User.GetObjectId();
        }
    }
}
