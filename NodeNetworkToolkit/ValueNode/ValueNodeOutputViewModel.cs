using System;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    public class ValueNodeOutputViewModel<T> : NodeOutputViewModel
    {
        static ValueNodeOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<ValueNodeOutputViewModel<T>>));
        }
        
        #region Value
        private IObservable<T> _value;
        public IObservable<T> Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        #endregion

        private ObservableAsPropertyHelper<T> _currentValue;
        public T CurrentValue => _currentValue.Value;

        public ValueNodeOutputViewModel()
        {
            this.WhenAnyObservable(vm => vm.Value).ToProperty(this, vm => vm.CurrentValue, out _currentValue);
        }
    }
}
