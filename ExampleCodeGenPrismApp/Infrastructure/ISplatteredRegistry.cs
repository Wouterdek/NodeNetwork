using System;
using ReactiveUI;

namespace ExampleCodeGenPrismApp.Infrastructure
{
    public interface ISplatteredRegistry
    {
        void Register<TView, TViewModel>() where TView : class, IViewFor;
        void Register(Type viewType, Type viewModelType);
    }
}
