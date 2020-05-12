﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Kernel;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ValueNode
{
    /// <summary>
    /// A node input that keeps a list of the latest values produced by all of the connected ValueNodeOutputViewModels.
    /// This input can take multiple connections, ValueNodeInputViewModel cannot.
    /// </summary>
    /// <typeparam name="T">The type of object this input can receive</typeparam>
    [DataContract]
    public class ValueListNodeInputViewModel<T> : NodeInputViewModel
    {
        static ValueListNodeInputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<ValueListNodeInputViewModel<T>>));
        }

        /// <summary>
        /// The current values of the outputs connected to this input
        /// </summary>
        [IgnoreDataMember] public IObservableList<T> Values { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is in design mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in design mode; otherwise, <c>false</c>.
        /// </value>
        [IgnoreDataMember] protected bool InDesignMode => DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

        public ValueListNodeInputViewModel()
        {
            MaxConnections = Int32.MaxValue;
            if (InDesignMode) return;

            ConnectionValidator = pending => new ConnectionValidationResult(
                pending.Output is ValueNodeOutputViewModel<T> ||
                pending.Output is ValueNodeOutputViewModel<IObservableList<T>>,
                null
            );

            var valuesFromSingles = Connections.Connect(c => c.Output is ValueNodeOutputViewModel<T>)
                .Transform(c => ((ValueNodeOutputViewModel<T>) c.Output))
                //Note: this line used to be
                //.AutoRefresh(output => output.CurrentValue)
                //which ignored changes where CurrentValue didn't change.
                //This caused problems when the value object isn't replaced, but one of its properties changes.
                .AutoRefreshOnObservable(output => output.Value)
                .Transform(output => output.CurrentValue, true)
                .Filter(v => v != null)
                .Select((IChangeSet<T> changes) =>
                {
                    if (changes.TotalChanges == changes.Replaced + changes.Refreshes)
                    {
                        bool allRefresh = true;
                        var newChanges = new ChangeSet<T>();
                        foreach (var change in changes)
                        {
                            if (change.Reason == ListChangeReason.Replace)
                            {
                                if (change.Type == ChangeType.Item)
                                {
                                    if (change.Item.Previous != change.Item.Current)
                                    {
                                        allRefresh = false;
                                        break;
                                    }
                                    newChanges.Add(new Change<T>(ListChangeReason.Refresh, change.Item.Current, change.Item.Previous, change.Item.CurrentIndex, change.Item.PreviousIndex));
                                }
                                else
                                {
                                    throw new Exception("Does this ever occur?");
                                }
                            }
                            else
                            {
                                newChanges.Add(change);
                            }
                        }

                        if (allRefresh) return newChanges;
                    }
                    return changes;
                });
            
            var valuesFromLists = Connections.Connect(c => c.Output is ValueNodeOutputViewModel<IObservableList<T>>)
                // Grab list of values from output, using switch to handle when the list object is replaced
                .Transform(c => ((ValueNodeOutputViewModel<IObservableList<T>>) c.Output).Value.Switch())
                // Materialize this changeset stream into a list (needed to make sure the next step is done dynamically)
                .AsObservableList()
                // Take the union of all values from all lists. This is done dynamically, so adding/removing new lists works as expected.
                .Or();

            Values = valuesFromSingles.Or(valuesFromLists).AsObservableList();
        }
    }
}
