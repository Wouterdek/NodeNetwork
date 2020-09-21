using System;
using System.Reactive;
using System.Reactive.Linq;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    /// <summary>
    /// An editor for ValueNodeInputViewModel or ValueNodeOutputViewModel.
    /// For inputs, this class can provide values when no connection is present.
    /// For outputs, this class can provide a way to configure the value produced by the output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueEditorViewModel<T> : NodeEndpointEditorViewModel
    {
        static ValueEditorViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeEndpointEditorView(), typeof(IViewFor<ValueEditorViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// The value currently set in the editor.
        /// </summary>
        public T Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        private T _value;
        #endregion

        #region ValueChanged
        /// <summary>
        /// Observable that produces an object when the value changes.
        /// </summary>
        public IObservable<T> ValueChanged { get; } 
        #endregion

        public ValueEditorViewModel()
        {
            ValueChanged = this.WhenAnyValue(vm => vm.Value);
        }
    }
}
