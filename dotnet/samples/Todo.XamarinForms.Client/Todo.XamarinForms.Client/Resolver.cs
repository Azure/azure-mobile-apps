using Autofac;

namespace Todo.XamarinForms.Client
{
    /// <summary>
    /// IoC / Dependency Injection resolver.
    /// </summary>
    public static class Resolver
    {
        private static IContainer container;

        public static void Initialize(IContainer container)
        {
            Resolver.container = container;
        }

        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }
    }
}
