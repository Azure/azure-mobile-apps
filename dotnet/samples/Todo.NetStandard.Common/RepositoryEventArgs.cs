using System;
using System.Collections.Generic;
using System.Text;

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// Event arguments for the RepositoryChanged Event handler.
    /// </summary>
    public class RepositoryEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="RepositoryEventArgs"/> instance.
        /// </summary>
        /// <param name="action">The action that was taken (Add, Update, Delete)</param>
        /// <param name="item">The item that was acted upon.</param>
        public RepositoryEventArgs(RepositoryAction action, TodoItem item)
        {
            Action = action;
            Item = item;
        }

        /// <summary>
        /// The action that was taken
        /// </summary>
        public RepositoryAction Action { get; }

        /// <summary>
        /// The item that was acted on.  In the case of the Add and Update, this is
        /// the new item.  In the case of the Delete action, it is the old item.
        /// </summary>
        public TodoItem Item { get; }
    }
}
