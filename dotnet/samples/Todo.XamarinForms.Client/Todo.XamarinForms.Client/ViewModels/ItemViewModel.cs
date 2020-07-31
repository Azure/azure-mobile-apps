using System;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.Client.ViewModels
{
    public class ItemViewModel : ViewModel
    {
        private TodoItemRepository repository;

        /// <summary>
        /// The todo-item that is currently being displayed
        /// </summary>
        public TodoItem Item { get; set; }

        public ItemViewModel(TodoItemRepository repository)
        {
            this.repository = repository;
            Item = new TodoItem() { Due = DateTime.Now.AddDays(1) };
        }

        /// <summary>
        /// An <see cref="ICommand"/> for saving the todo item.
        /// </summary>
        public ICommand Save => new Command(async () =>
        {
            await repository.AddOrUpdateItemAsync(Item);
            await Navigation.PopAsync();
        });
    }
}
