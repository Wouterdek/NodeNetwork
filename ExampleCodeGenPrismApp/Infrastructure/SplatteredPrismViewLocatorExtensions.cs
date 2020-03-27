using Prism.Ioc;
using ReactiveUI;
using Splat;

namespace ExampleCodeGenPrismApp.Infrastructure
{
    public static class SplatteredPrismViewLocatorExtensions
    {
        public static ISplatteredRegistry UseSplatteredPrismViewLocator(this IContainerProvider container)
        {
            var splatteredLocator = SplatteredPrismViewLocator.Current;
            splatteredLocator.Container = container;

            // Override the default ViewLocator
            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.InitializeReactiveUI();
            Locator.CurrentMutable.RegisterLazySingleton(() => splatteredLocator, typeof(IViewLocator));

            return splatteredLocator;
        }
    }
}
