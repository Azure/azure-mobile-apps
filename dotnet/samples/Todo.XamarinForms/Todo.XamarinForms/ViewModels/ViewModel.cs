using Todo.NetStandard.Common;
using Xamarin.Forms;

namespace Todo.XamarinForms.ViewModels
{
    public abstract class ViewModel : BindableObject
    {
        /// <summary>
        /// Creates a new <see cref="ViewModel"/> instance with no pre-population.
        /// </summary>
        protected ViewModel() { }

        /// <summary>
        /// Creates a new <see cref="ViewModel"/> instance with pre-populated
        /// Navigation and Repository objects.
        /// </summary>
        /// <param name="navigation">The Navigation property value</param>
        /// <param name="repository">The Repository property value</param>
        public ViewModel(INavigation navigation, ITodoRepository repository)
        {
            Navigation = navigation;
            Repository = repository;
        }

        /// <summary>
        /// Provided by the NavigationPage (or NavigableElement) to
        /// allow navigation.
        /// </summary>
        public INavigation Navigation { get; set; }

        /// <summary>
        /// Proides access to the repository that is used to power the
        /// application
        /// </summary>
        public ITodoRepository Repository { get; set; }
    }
}
