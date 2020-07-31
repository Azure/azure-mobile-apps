using Azure.Mobile.Client;

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// Model for a single TodoItem.
    /// </summary>
    public class TodoItem : TableData
    {
        /// <summary>
        /// The text or title of the TodoItem
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// True if the TodoItem is completed.
        /// </summary>
        public bool Complete { get; set; }
    }
}
