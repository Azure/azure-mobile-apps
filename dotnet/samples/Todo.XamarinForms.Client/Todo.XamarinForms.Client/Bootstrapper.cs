using Autofac;
using System.Linq;
using System.Reflection;
using Todo.NetStandard.Common;
using Todo.XamarinForms.Client.ViewModels;
using Xamarin.Forms;

namespace Todo.XamarinForms.Client
{
    public class Bootstrapper
    {
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public static void Initialize()
        {
            var instance = new Bootstrapper();
        }

        public Bootstrapper()
        {
            InitializeDI();
            FinishInitialization();
        }

        /// <summary>
        /// Register each of the Page views and view models for dependency injection.
        /// </summary>
        protected virtual void InitializeDI()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            ContainerBuilder = new ContainerBuilder();
            foreach (var type in currentAssembly.DefinedTypes.Where(e => IsPage(e) || IsViewModel(e)))
            {
                ContainerBuilder.RegisterType(type.AsType());
            }
            ContainerBuilder.RegisterType<TodoItemRepository>().SingleInstance();
        }

        private void FinishInitialization()
        {
            var container = ContainerBuilder.Build();
            Resolver.Initialize(container);
        }

        /// <summary>
        /// Is the referenced type a Page view.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsPage(TypeInfo e) => e.IsSubclassOf(typeof(Page));

        /// <summary>
        /// Is the referenced type a View model.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsViewModel(TypeInfo e) => e.IsSubclassOf(typeof(ViewModel));
    }
}
