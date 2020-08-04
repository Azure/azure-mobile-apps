using System.Threading.Tasks;
using System.Windows.Input;
using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.ViewModels
{
    public class TodoItemViewModel : ViewModel
    {
        private TodoItem item;

        public TodoItemViewModel(INavigation navigation, ITodoRepository repository, TodoItem todoItem)
            : base(navigation, repository)
        {
            Item = todoItem;
        }

        #region Bindable Properties
        /// <summary>
        /// The item currently being edited.
        /// </summary>
        public TodoItem Item
        {
            get => item;
            set { item = value; OnPropertyChanged(nameof(Item)); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command triggered when the user pressed Cancel.
        /// </summary>
        public ICommand CancelCommand => new Command(async () => await Navigation.PopModalAsync());

        /// <summary>
        /// Command triggered when the user pressed OK
        /// </summary>
        public ICommand SaveItemCommand => new Command(async () => await SaveItemAsync());
        #endregion

        /// <summary>
        /// Perform the backend processing for saving the item, then pop back to the list.
        /// </summary>
        /// <returns></returns>
        private async Task SaveItemAsync()
        {
            await Repository.SaveTodoItemAsync(Item);
            await Navigation.PopModalAsync();
        }
    }
}
