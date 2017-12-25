using System;
using System.Reactive;
using System.Reactive.Linq;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    public class ValueEditorViewModel<T> : NodeEndpointEditorViewModel
    {
        static ValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeEndpointEditorViewModel(), typeof(IViewFor<ValueEditorViewModel<T>>));
        }

        #region Value
        private T _value;
        public T Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        #endregion

        public IObservable<T> ValueChanged { get; }

        public ValueEditorViewModel()
        {
            Changed = this.WhenAnyValue(vm => vm.Value).Select(_ => Unit.Default);
            ValueChanged = this.WhenAnyValue(vm => vm.Value);
        }
    }
}
