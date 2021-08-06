using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
            NNViewRegistrar.AddRegistration(() => new NodeInputView(), typeof(IViewFor<ValueNodeInputViewModel<T>>));
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
            MaxConnections = 1;
            ConnectionValidator = pending => new ConnectionValidationResult(pending.Output is ValueNodeOutputViewModel<T>, null);

            var connectedValues = GenerateConnectedValuesBinding(connectionChangedValidationAction, connectedValueChangedValidationAction);
            
            var localValues = this.WhenAnyValue(vm => vm.Editor)
                .Select(e =>
                {
                    if (e == null)
                    {
                        return Observable.Return(default(T));
                    }
                    else if (!(e is ValueEditorViewModel<T>))
                    {
                        throw new Exception($"The endpoint editor is not a subclass of ValueEditorViewModel<{typeof(T).Name}>");
                    }
                    else
                    {
                        return ((ValueEditorViewModel<T>)e).ValueChanged;
                    }
                })
                .Switch();

            var valueChanged = Observable.CombineLatest(connectedValues, localValues,
                    (connectedValue, localValue) => Connections.Count == 0 ? localValue : connectedValue
                ).Publish();
            valueChanged.Connect();
            valueChanged.ToProperty(this, vm => vm.Value, out _value);
            
            ValueChanged = Observable
                .Defer(() => Observable.Return(Value))
                .Concat(valueChanged);
        }

        private IObservable<T> GenerateConnectedValuesBinding(ValidationAction connectionChangedValidationAction, ValidationAction connectedValueChangedValidationAction)
        {
            var onConnectionChanged = this.Connections.Connect().Select(_ => Unit.Default).StartWith(Unit.Default)
                .Select(_ => Connections.Count == 0 ? null : Connections.Items.First());

            //On connection change
            IObservable<IObservable<T>> connectionObservables;
            if (connectionChangedValidationAction != ValidationAction.DontValidate)
            {
                //Either run network validation
                IObservable<NetworkValidationResult> postValidation = onConnectionChanged
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
                    {
                        if (Connections.Count == 0)
                        {
                            return Observable.Return(default(T));
                        }
                        else if(validation.NetworkIsTraversable)
                        {
                            IObservable<T> connectedObservable =
                                ((ValueNodeOutputViewModel<T>) Connections.Items.First().Output).Value;
                            if (connectedObservable == null)
                            {
                                throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                            }
                            return connectedObservable;
                        }
                        else
                        {
                            return Observable.Return(default(T));
                        }
                    });
                }
                else
                {
                    //Grab the values observable from the connected output
                    connectionObservables = postValidation
                        .Select(_ =>
                        {
                            if (Connections.Count == 0)
                            {
                                return Observable.Return(default(T));
                            }
                            else
                            {
                                IObservable<T> connectedObservable =
                                    ((ValueNodeOutputViewModel<T>)Connections.Items.First().Output).Value;
                                if (connectedObservable == null)
                                {
                                    throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                                }
                                return connectedObservable;
                            }
                        });
                }
            }
            else
            {
                //Or just grab the values observable from the connected output
                connectionObservables = onConnectionChanged.Select(con =>
                {
                    if (con == null)
                    {
                        return Observable.Return(default(T));
                    }
                    else
                    {
                        IObservable<T> connectedObservable =
                            ((ValueNodeOutputViewModel<T>)con.Output).Value;
                        if (connectedObservable == null)
                        {
                            throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                        }
                        return connectedObservable;
                    }
                });
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
                    if (Connections.Count == 0
                        || connectionChangedValidationAction == ValidationAction.PushDefaultValue && !validation.NetworkIsTraversable
                        || connectedValueChangedValidationAction == ValidationAction.PushDefaultValue && !validation.IsValid)
                    {
                        //Push default(T) if the network isn't valid
                        return default;
                    }

                    //Or just ignore the validation and push the value as is
                    return ((ValueNodeOutputViewModel<T>) this.Connections.Items.First().Output).CurrentValue;
                });
            }

            return connectedValues;
        }
    }
}
