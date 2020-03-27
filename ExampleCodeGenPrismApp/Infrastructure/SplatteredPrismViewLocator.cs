using System;
using System.Collections.Generic;
using System.Text;
using Prism.Ioc;
using ReactiveUI;

namespace ExampleCodeGenPrismApp.Infrastructure
{
    public class SplatteredPrismViewLocator : IViewLocator, ISplatteredRegistry
    {
        private readonly IDictionary<Type, Type> _viewMap;
        
        public IContainerProvider Container { get; set; }

        private static SplatteredPrismViewLocator _instance;
        public static SplatteredPrismViewLocator Current
        {
            get { return _instance ??= new SplatteredPrismViewLocator(); }
        }

        private SplatteredPrismViewLocator()
        {
            _viewMap = new Dictionary<Type, Type>();
        }

        public IViewFor ResolveView<T>(T viewModel, string contract = null) where T : class
        {
            var viewModelType = viewModel.GetType();

            if (_viewMap.ContainsKey(viewModelType))
                return ResolveFromContainer(_viewMap[viewModelType]);

            if (viewModelType.IsGenericType)
            {
                var genericViewModelType = viewModelType.GetGenericTypeDefinition();

                if (_viewMap.ContainsKey(genericViewModelType))
                    return ResolveFromContainer(_viewMap[genericViewModelType]);
            }

            return null;
        }

        private IViewFor ResolveFromContainer(Type viewType)
        {
            return Container.Resolve(viewType) as IViewFor;
        }

        public void Register<TView, TViewModel>() where TView : class, IViewFor
        {
            var viewType = typeof(TView);
            var viewModelType = typeof(TViewModel);

            Register(viewType, viewModelType);
        }

        public void Register(Type viewType, Type viewModelType)
        {
            if (!_viewMap.ContainsKey(viewModelType))
                _viewMap.Add(viewModelType, viewType);
        }
    }
}
