using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace NodeNetwork.Utilities
{
    public static class ReactiveExtensions
    {
        /// <summary>
        /// Takes each list produced by this observable and mirrors its contents in the target list.
        /// The target list is modified, not replaced.
        /// The type of the target list property is IReadOnlyReactiveList because it doesn't make sense to have a mutible list
        /// if this binding keeps changing the contents of the list, but the type of the actual object should be ReactiveList 
        /// so the list can be modified by this binding.
        /// </summary>
        /// <typeparam name="TObj">The type of viewmodel</typeparam>
        /// <typeparam name="TListItem">The type of object contained in the list</typeparam>
        /// <param name="data">The observable to take lists from.</param>
        /// <param name="target">The viewmodel that is used as a base for finding the target list property</param>
        /// <param name="property">The IReactiveList property that will be modified.</param>
        /// <returns>A disposable to break the binding</returns>
        public static IDisposable BindListContents<TObj, TListItem>(this IObservable<IList<TListItem>> data,
            TObj target, Expression<Func<TObj, IReadOnlyReactiveList<TListItem>>> property)
        {
            IObservable<IReadOnlyReactiveList<TListItem>> targetListObservable = target.WhenAnyValue(property);

            return Observable.CombineLatest(targetListObservable, data, (a, b) => (TargetList: a, SourceList: b))
                .Subscribe(t =>
                {
                    IReactiveList<TListItem> latestTargetList = t.TargetList as IReactiveList<TListItem>;
                    IList<TListItem> latestData = t.SourceList;

                    if (latestTargetList == null)
                    {
                        return;
                    }

                    if (latestData == null)
                    {
                        latestTargetList.Clear();
                        return;
                    }

                    var changes = LongestCommonSubsequence.GetChanges(latestTargetList, latestData).ToArray();
                    if (changes.Length == 0)
                    {
                        return;
                    }

                    using (changes.Length > 1 ? latestTargetList.SuppressChangeNotifications() : Disposable.Empty)
                    {
                        foreach ((int index, TListItem item, LongestCommonSubsequence.ChangeType changeType) change in
                            changes)
                        {
                            if (change.changeType == LongestCommonSubsequence.ChangeType.Removed)
                            {
                                latestTargetList.RemoveAt(change.index);
                            }
                            else if (change.changeType == LongestCommonSubsequence.ChangeType.Added)
                            {
                                latestTargetList.Insert(change.index, change.item);
                            }
                        }
                    }
                });
        }
        
        public static (IReadOnlyReactiveList<R> List, IDisposable Binding) CreateDerivedList<T, R>(
            this IObservable<IReactiveList<T>> obs, Func<T, bool> filter, Func<T, R> selector)
        {
            ReactiveList<R> result = new ReactiveList<R>();
            IDisposable latestBinding = null;

            // Dispose the binding when we receive a null list.
            var cleanupSubscription = obs.Where(l => l == null).Subscribe(_ => latestBinding?.Dispose());
                
            var bindingSubscription = obs
                // Create a new derived list when the source list changes. (and is not null)
                .Where(l => l != null)
                .Select(l => l.CreateDerivedCollection(selector, filter))
                // Mirror the contents of the derived list to result
                .Select(list =>
                {
                    var binding = list.Changed.Subscribe(change =>
                    {
                        switch (change.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                int index = change.NewStartingIndex != -1 ? change.NewStartingIndex : result.Count;
                                result.InsertRange(index, change.NewItems.Cast<R>());
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                if (change.OldStartingIndex != -1)
                                {
                                    result.RemoveRange(change.OldStartingIndex, change.OldItems.Count);
                                }
                                else
                                {
                                    result.RemoveAll(change.OldItems.Cast<R>());
                                }
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                if (change.OldStartingIndex == change.NewStartingIndex &&
                                    change.OldStartingIndex != -1)
                                {
                                    result.RemoveRange(change.OldStartingIndex, change.OldItems.Count);
                                    result.InsertRange(change.OldStartingIndex, change.NewItems.Cast<R>());
                                }
                                break;

                            case NotifyCollectionChangedAction.Move:
                                result.RemoveRange(change.OldStartingIndex, change.OldItems.Count);
                                result.InsertRange(change.NewStartingIndex, change.NewItems.Cast<R>());
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                result.Clear();
                                result.AddRange(list);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                    return (binding, list);
                })
                // When the parent network changes, we will create a new derived list and so we dispose the old one.
                .Select(t => new CompositeDisposable(t.Item1, t.Item2))
                .Subscribe(newBinding =>
                {
                    latestBinding?.Dispose();
                    latestBinding = newBinding;
                });

            var disposable = Disposable.Create(() =>
            {
                cleanupSubscription.Dispose();
                bindingSubscription.Dispose();
                latestBinding?.Dispose();
            });
            return (result, disposable);
        }

        /// <summary>
        /// Takes an observable of T values and returns an observable of tuples of T values containing the latest value and the previous value.
        /// The first item in the source observable produces a tuple with the previous value set to default(T).
        /// </summary>
        /// <typeparam name="T">The type of object in the observable</typeparam>
        /// <param name="obs">The source observable</param>
        /// <returns>The resulting observable</returns>
        public static IObservable<(T OldValue, T NewValue)> PairWithPreviousValue<T>(this IObservable<T> obs)
        {
            return obs.Scan((oldValue: default(T), newValue: default(T)), (pair, newVal) => (pair.newValue, newVal));
        }

        /// <summary>
        /// Apply the specified observableSelector to every item that is added to the list,
        /// and automatically unsubscribes the resulting observable when the item is removed from the list.
        /// </summary>
        /// <typeparam name="V">The value produced by the observable returned by observableSelector.</typeparam>
        /// <param name="observableSelector">A function that maps each element on an observable.</param>
        /// <returns>An observable of the elements that are emitted along with the item that produced it.</returns>
        public static IObservable<(T Element, V Value)> ObserveEach<T, V>(this IReactiveList<T> list, Func<T, IObservable<V>> observableSelector)
        {
            return list.ObserveWhere(observableSelector, t => true);
        }

        /// <summary>
        /// Apply the specified observableSelector to every item that is added to the list and matches filter,
        /// and automatically unsubscribes the resulting observable when the item is removed from the list.
        /// </summary>
        /// <typeparam name="V">The value produced by the observable returned by observableSelector.</typeparam>
        /// <param name="observableSelector">A function that maps each matching element on an observable.</param>
        /// <param name="filter">A predicate that specifies whether or not this specific element should be observed</param>
        /// <returns>An observable of the elements that are emitted along with the item that produced it.</returns>
        public static IObservable<(T Element, V Value)> ObserveWhere<T, V>(this IReactiveList<T> list, Func<T, IObservable<V>> observableSelector, Func<T, bool> filter)
        {
            if (observableSelector == null)
            {
                throw new ArgumentNullException(nameof(observableSelector));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            // Take all items that are currently in the list (values and corresponding index), 
            // including all that will be added in the future.
            // On reset, pretend all items in the list are new and re-add them.
            var currentContents = list.ToObservable();
            IObservable<T> items = currentContents.Concat(
                Observable.Merge(
                    list.ItemsAdded,
                    list.ShouldReset.SelectMany(_ => list)
                )
            );

            // Select the target observable using observableSelector and return
            // values from it until the item is removed from this list.
            // On reset, dispose all previous subscriptions.
            return items.Where(e => filter(e)).SelectMany(newElem =>
                observableSelector(newElem)
                    .TakeUntil(
                        Observable.Merge(
                            list.ItemsRemoved.Where(deletedElem => Object.Equals(deletedElem, newElem)).Select(_ => Unit.Default),
                            list.ShouldReset
                        )
                    )
                    .Select(val => (newElem, val))
            );
        }
    }
}
