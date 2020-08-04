using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.ViewModels
{
    public abstract class ViewModel : BindableObject
    {
        protected ViewModel() { }

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModel"/> - this
        /// is normally called as base(navigation,repository) from the 
        /// constructor of the inherited class.
        /// </summary>
        /// <param name="navigation">The navigation context</param>
        /// <param name="repository">The data repository</param>
        public ViewModel(INavigation navigation, ITodoRepository repository)
        {
            Navigation = navigation;
            Repository = repository;
        }

        /// <summary>
        /// Reference to the current navigation context
        /// </summary>
        public INavigation Navigation { get; set; }

        /// <summary>
        /// Reference to the current data repository
        /// </summary>
        public ITodoRepository Repository { get; set; }

        #region Bindable Properties
        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        #endregion
    }
}
