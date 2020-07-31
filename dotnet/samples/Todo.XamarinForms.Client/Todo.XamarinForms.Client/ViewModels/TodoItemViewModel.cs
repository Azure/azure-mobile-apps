using System;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.Client.ViewModels
{
    public class TodoItemViewModel : ViewModel
    {
        public TodoItemViewModel(TodoItem item)
        {
            Item = item;
        }

        /// <summary>
        /// Event handler list called when the item has changed state.
        /// </summary>
        public event EventHandler ItemStatusChanged;

        /// <summary>
        /// The underlying item.
        /// </summary>
        public TodoItem Item { get; private set; }

        /// <summary>
        /// Converts the status of the task to a string.
        /// </summary>
        public string StatusText => Item.Completed ? "Incomplete" : "Completed";

        /// <summary>
        /// A <see cref="ICommand"/> for toggling the state of the task
        /// </summary>
        public ICommand ToggleCompleted => new Command((arg) => { Item.Completed = !Item.Completed; ItemStatusChanged?.Invoke(this, new EventArgs()); });

    }
}
