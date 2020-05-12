using System;
using System.Runtime.Serialization;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    /// <summary>
    /// An editor for ValueNodeInputViewModel or ValueNodeOutputViewModel.
    /// For inputs, this class can provide values when no connection is present.
    /// For outputs, this class can provide a way to configure the value produced by the output.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ValueEditorViewModel<T> : NodeEndpointEditorViewModel
    {
        static ValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeEndpointEditorViewModel(), typeof(IViewFor<ValueEditorViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// The value currently set in the editor.
        /// </summary>
        [DataMember]
        public T Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        [IgnoreDataMember] private T _value;
        #endregion

        #region ValueChanged
        /// <summary>
        /// Observable that produces an object when the value changes.
        /// </summary>
        [IgnoreDataMember] public IObservable<T> ValueChanged { get; } 
        #endregion

        public ValueEditorViewModel()
        {
            ValueChanged = this.WhenAnyValue(vm => vm.Value);
        }
    }
}
