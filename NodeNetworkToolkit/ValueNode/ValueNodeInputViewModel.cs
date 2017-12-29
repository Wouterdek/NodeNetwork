using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    /// <summary>
    /// A node input that keeps track of the latest value produced by either the connected ValueNodeOutputViewModel, 
    /// or the ValueEditorViewModel in the Editor property.
    /// </summary>
    /// <typeparam name="T">The type of object this input can receive</typeparam>
    public class ValueNodeInputViewModel<T> : NodeInputViewModel
    {
        static ValueNodeInputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<ValueNodeInputViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// The value currently associated with this input.
        /// If the input is not connected, the value is taken from ValueEditorViewModel.Value in the Editor property.
        /// If the input is connected, the value is taken from ValueNodeOutputViewModel.LatestValue unless the network is not traversable.
        /// Note that this value may be equal to default(T) if there is an error somewhere.
        /// </summary>
        public T Value => _value.Value;
        private readonly ObservableAsPropertyHelper<T> _value;
        #endregion

        #region ValueChanged
        /// <summary>
        /// An observable that fires when the input value changes. 
        /// This may be because of a connection change, editor value change, network validation change, ...
        /// </summary>
        public IObservable<T> ValueChanged { get; } 
        #endregion

        /// <summary>
        /// Action that should be taken based on the validation result
        /// </summary>
        public enum ValidationAction
        {
            /// <summary>
            /// Don't run the validation. (LatestValidation is not updated)
            /// </summary>
            DontValidate,
            /// <summary>
            /// Run the validation, but ignore the result and assume the network is valid.
            /// </summary>
            IgnoreValidation,
            /// <summary>
            /// Run the validation and if the network is invalid then wait until it is valid.
            /// </summary>
            WaitForValid,
            /// <summary>
            /// Run the validation and if the network is invalid then make default(T) the current value.
            /// </summary>
            PushDefaultValue
        }

        /// <summary>
        /// Constructs a new ValueNodeInputViewModel with the specified ValidationActions. 
        /// The default values are carefully chosen and should probably not be changed unless you know what you are doing.
        /// </summary>
        /// <param name="connectionChangedValidationAction">The validation behaviour when the connection of this input changes.</param>
        /// <param name="connectedValueChangedValidationAction">The validation behaviour when the value of this input changes.</param>
        public ValueNodeInputViewModel(
            ValidationAction connectionChangedValidationAction = ValidationAction.PushDefaultValue, 
            ValidationAction connectedValueChangedValidationAction = ValidationAction.IgnoreValidation
        )
        {
            Editor = new ValueEditorViewModel<T>();
            ConnectionValidator = pending => new ConnectionValidationResult(pending.Output is ValueNodeOutputViewModel<T>, null);

            var connectedValues = GenerateConnectedValuesBinding(connectionChangedValidationAction, connectedValueChangedValidationAction);
            
            var localValues = this.WhenAnyObservable(vm => vm.Editor.Changed).Select(_ =>
            {
                ValueEditorViewModel<T> valueEditor = Editor as ValueEditorViewModel<T>;
                if (valueEditor == null)
                {
                    throw new Exception($"The endpoint editor is not a subclass of ValueEditorViewModel<{typeof(T).Name}>");
                }
                return valueEditor.Value;
            });

            var valueChanged = Observable.CombineLatest(connectedValues, localValues,
                    (connectedValue, localValue) => Connection == null ? localValue : connectedValue
                )
                .Multicast(new Subject<T>());
            valueChanged.Connect();
            valueChanged.ToProperty(this, vm => vm.Value, out _value);
            ValueChanged = Observable.Create<T>(observer =>
            {
                observer.OnNext(Value);
                return valueChanged.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            });
        }

        private IObservable<T> GenerateConnectedValuesBinding(ValidationAction connectionChangedValidationAction, ValidationAction connectedValueChangedValidationAction)
        {
            //On connection change
            IObservable<IObservable<T>> connectionObservables;
            if (connectionChangedValidationAction != ValidationAction.DontValidate)
            {
                //Either run network validation
                IObservable<NetworkValidationResult> postValidation = this.WhenAnyValue(vm => vm.Connection)
                    .SelectMany(con => Parent?.Parent?.UpdateValidation.Execute() ?? Observable.Return(new NetworkValidationResult(true, true, null)));

                if (connectionChangedValidationAction == ValidationAction.WaitForValid)
                {
                    //And wait until the validation is successful
                    postValidation = postValidation.SelectMany(validation =>
                        validation.NetworkIsTraversable
                            ? Observable.Return(validation)
                            : Parent.Parent.Validation.FirstAsync(val => val.NetworkIsTraversable));
                }

                if (connectionChangedValidationAction == ValidationAction.PushDefaultValue)
                {
                    //Or push a single default(T) if the validation fails
                    connectionObservables = postValidation.Select(validation =>
                        Connection == null
                            ? Observable.Return(default(T))
                            : (validation.NetworkIsTraversable
                                ? ((ValueNodeOutputViewModel<T>) Connection.Output).Value
                                : Observable.Return(default(T))));
                }
                else
                {
                    //Grab the values observable from the connected output
                    connectionObservables = postValidation.Select(_ => Connection)
                        .Select(con =>
                            con == null ? Observable.Return(default(T)) : ((ValueNodeOutputViewModel<T>) con.Output).Value);
                }
            }
            else
            {
                //Or just grab the values observable from the connected output
                connectionObservables = this.WhenAnyValue(vm => vm.Connection)
                    .Select(con =>
                        con == null ? Observable.Return(default(T)) : ((ValueNodeOutputViewModel<T>) con.Output).Value);
            }
            IObservable<T> connectedValues = connectionObservables.SelectMany(c => c);

            //On connected output value change, either just push the value as is
            if (connectedValueChangedValidationAction != ValidationAction.DontValidate)
            {
                //Or run a network validation
                IObservable<NetworkValidationResult> postValidation = connectedValues.SelectMany(v =>
                    Parent?.Parent?.UpdateValidation.Execute() ?? Observable.Return(new NetworkValidationResult(true, true, null)));
                if (connectedValueChangedValidationAction == ValidationAction.WaitForValid)
                {
                    //And wait until the validation is successful
                    postValidation = postValidation.SelectMany(validation =>
                        validation.IsValid
                            ? Observable.Return(validation)
                            : Parent.Parent.Validation.FirstAsync(val => val.IsValid));
                }

                connectedValues = postValidation.Select(validation =>
                {
                    if (Connection == null
                        || connectionChangedValidationAction == ValidationAction.PushDefaultValue && !validation.NetworkIsTraversable
                        || connectedValueChangedValidationAction == ValidationAction.PushDefaultValue && !validation.IsValid)
                    {
                        //Push default(T) if the network isn't valid
                        return default(T);
                    }

                    //Or just ignore the validation and push the value as is
                    return ((ValueNodeOutputViewModel<T>) this.Connection.Output).CurrentValue;
                });
            }

            return connectedValues;
        }
    }
}
