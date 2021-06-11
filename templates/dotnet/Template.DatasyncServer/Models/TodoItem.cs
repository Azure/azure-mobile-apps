using Microsoft.AspNetCore.Datasync.EFCore;

namespace Template.DatasyncServer.Models
{
    /// <summary>
    /// A DTO (data transfer object) for the TodoItems table.
    /// </summary>
    public class TodoItem : EntityTableData
    {
        /// <summary>
        /// Text of the Todo Item
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Is the item complete?
        /// </summary>
        public bool Complete { get; set; }
    }
}