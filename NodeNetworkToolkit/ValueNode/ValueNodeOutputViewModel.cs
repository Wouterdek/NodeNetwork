﻿using System;
using System.Runtime.Serialization;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    /// <summary>
    /// A viewmodel for a node output that produces a value based on the inputs.
    /// </summary>
    /// <typeparam name="T">The type of object produced by this output.</typeparam>
    [DataContract]
    public class ValueNodeOutputViewModel<T> : NodeOutputViewModel
    {
        static ValueNodeOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<ValueNodeOutputViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// Observable that produces the value every time it changes.
        /// </summary>
        [IgnoreDataMember]
        public IObservable<T> Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        [IgnoreDataMember] private IObservable<T> _value;
        #endregion

        #region CurrentValue
        /// <summary>
        /// The latest value produced by this output.
        /// </summary>
        [DataMember] public T CurrentValue => _currentValue.Value;
        [IgnoreDataMember] private ObservableAsPropertyHelper<T> _currentValue;
        #endregion

        public ValueNodeOutputViewModel()
        {
            this.WhenAnyObservable(vm => vm.Value).ToProperty(this, vm => vm.CurrentValue, out _currentValue);
        }
    }
}
