using Microsoft.AzureMobile.Server.EFCore;

namespace azuremobileserver.Db
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