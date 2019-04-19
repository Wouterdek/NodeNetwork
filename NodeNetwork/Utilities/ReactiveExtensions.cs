using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Legacy;
using PropertyChangingEventArgs = ReactiveUI.PropertyChangingEventArgs;
using PropertyChangingEventHandler = ReactiveUI.PropertyChangingEventHandler;

namespace NodeNetwork.Utilities
{
    public static class ReactiveExtensions
    {
		/// <summary>
		/// Create a one way list binding from the viewmodel to the view.
		/// The view list property will be automatically updated to reflect
		/// the viewmodel source list property.
		/// </summary>
		/// <typeparam name="TView">The type of the view</typeparam>
		/// <typeparam name="TViewModel">The type of the viewmodel</typeparam>
		/// <typeparam name="TData">The type of the data stored in the list</typeparam>
		/// <typeparam name="TProperty">The type of the target property in the view</typeparam>
		/// <param name="self">The view used for the binding</param>
		/// <param name="vmDummy">A dummy viewmodel parameter, used to infer the viewmodel type</param>
		/// <param name="vmProperty">The source property in the viewmodel that contains the list</param>
		/// <param name="viewProperty">The target property in the view to bind the list to.</param>
		/// <returns>An object that when disposed, disconnects the binding.</returns>
		public static IDisposable BindList<TView, TViewModel, TData, TProperty>(
			    this TView self,
				TViewModel vmDummy,
			    Expression<Func<TViewModel, IObservableList<TData>>> vmProperty,
			    Expression<Func<TView, TProperty>> viewProperty
		    )
		    where TView: class, IViewFor<TViewModel> 
		    where TViewModel: class
	    {
		    IDisposable lastBinding = null;

		    return 
			    // Get latest viewmodel
			    self.WhenAnyValue(v => v.ViewModel)
			    .Where(vm => vm != null)
				// Get latest non-null list from viewmodel property
			    .Select(vm => vm.WhenAnyValue(vmProperty))
			    .Switch()
			    .Where(sourceList => sourceList != null)
			    // Clean up last list binding
				.Do(p => lastBinding?.Dispose())
				// Create new list binding
				.Select(sourceList =>
			    {
				    lastBinding = sourceList.Connect().Bind(out var list).Subscribe();
				    return list;
			    })
				// When the observable is disposed, dispose the list binding too
			    .Finally(() =>
			    {
				    lastBinding?.Dispose();
				})
				// Bind the new bindable list to the view property
			    .BindTo(self, viewProperty);
	    }

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
            TObj target, Expression<Func<TObj, IReadOnlyReactiveList<TListItem>>> property) where TObj : class
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
                        foreach (var (index, item, changeType) in changes)
                        {
                            if (changeType == LongestCommonSubsequence.ChangeType.Removed)
                            {
                                latestTargetList.RemoveAt(index);
                            }
                            else if (changeType == LongestCommonSubsequence.ChangeType.Added)
                            {
                                latestTargetList.Insert(index, item);
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
        public static IObservable<(T Element, V Value)> ObserveEach<T, V>(this IReadOnlyReactiveList<T> list, Func<T, IObservable<V>> observableSelector)
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
	    /// <param name="onAdd">Action that is run each time an item is added to the list.</param>
	    /// <param name="onRemove">Action that is run each time an item is removed from the list.</param>
	    /// <returns>An observable of the elements that are emitted along with the item that produced it.</returns>
	    public static IObservable<(T Element, V Value)> ObserveWhere<T, V>(
		    this IReadOnlyReactiveList<T> list, 
		    Func<T, IObservable<V>> observableSelector, 
		    Func<T, bool> filter,
		    Action<T> onAdd = null, 
		    Action<T> onRemove = null
		)
	    {
		    if (observableSelector == null)
		    {
			    throw new ArgumentNullException(nameof(observableSelector));
		    }
		    if (filter == null)
		    {
			    throw new ArgumentNullException(nameof(filter));
		    }
			onAdd = onAdd ?? (_ => { });
		    onRemove = onRemove ?? (_ => { });

			// Take all items that are currently in the list (values and corresponding index), 
			// including all that will be added in the future.
			// On reset, pretend all items in the list are new and re-add them.
			// Defer should be used because we want to start with a snapshot of the list contents
			// when the observable is subscribed, instead of when the observable is created.
			IObservable<T> items = Observable.Defer(() =>
			    Observable.Merge(
				    list.ItemsAdded,
				    list.ShouldReset.SelectMany(_ => list.ToArray())
			    ).StartWith(list.ToArray())
		    );

		    // Select the target observable using observableSelector and return
		    // values from it until the item is removed from this list.
		    // On reset, dispose all previous subscriptions.
		    return items.Where(filter).Do(onAdd)
			    .SelectMany(newElem =>
					observableSelector(newElem)
					    .TakeUntil(
						    Observable.Merge(
							    list.ItemsRemoved.Where(deletedElem => Object.Equals(deletedElem, newElem)).Select(_ => Unit.Default),
							    list.ShouldReset
						    ).Do(_ => onRemove(newElem))
					    )
					    .Select(val => (newElem, val))
				);
	    }

		public static (IReadOnlyReactiveList<V> List, IDisposable Binding) ObserveLatestToList<T, V>(
		    this IReadOnlyReactiveList<T> list, Func<T, IObservable<V>> observableSelector, Func<T, bool> filter)
	    {
		    if (observableSelector == null)
		    {
			    throw new ArgumentNullException(nameof(observableSelector));
		    }
		    if (filter == null)
		    {
			    throw new ArgumentNullException(nameof(filter));
		    }

		    IReactiveList<(T Key, V Value)> valueStore = new ReactiveList<(T, V)>();

		    int FindIndex(T key)
		    {
			    int index = valueStore.TakeWhile(v => !Object.Equals(v.Key, key)).Count();
			    return index == valueStore.Count ? -1 : index;
		    }
			
		    var binding = list.ObserveWhere(observableSelector, filter, e => { }, e =>
			    {
					int index = FindIndex(e);
				    if (index != -1)
				    {
						valueStore.RemoveAt(index);
					}
			    })
			    .Subscribe(t =>
			    {
					int index = FindIndex(t.Element);
				    if (index == -1)
				    {
						valueStore.Add(t);
					}
				    else
				    {
					    valueStore[index] = t;
				    }
				});

		    var resultList = valueStore.CreateDerivedCollection(t => t.Value);
			return (resultList, binding);
	    }

		/// <summary>
		/// Creates a readonly wrapper around the specified reactive list.
		/// Note that this does not create a immutable copy: changes to the original list
		/// will be reflected in changes to this list.
		/// </summary>
		/// <typeparam name="T">The type of content in the list.</typeparam>
		/// <param name="list">The list to wrap.</param>
		/// <returns>A readonly version of the list.</returns>
		public static IReadOnlyReactiveList<T> AsReadOnly<T>(this IReactiveList<T> list)
        {
            if (list is ReactiveList<T> impl)
            {
                return impl;
            }
            else
            {
                return new ReadOnlyReactiveListWrapper<T>(list);
            }
        }

        /// <summary>
        /// Wrapper class to create IReadOnlyReactiveList from IReactiveList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ReadOnlyReactiveListWrapper<T> : IReadOnlyReactiveList<T>
        {
            private readonly IReactiveList<T> _list;

            public ReadOnlyReactiveListWrapper(IReactiveList<T> list)
            {
                _list = list;
            }

            public void Reset()
            {
                _list.Reset();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _list).GetEnumerator();
            }

            public int Count => _list.Count;

            public IDisposable SuppressChangeNotifications()
            {
                return _list.SuppressChangeNotifications();
            }

            public IObservable<T> ItemsAdded => _list.ItemsAdded;

            public IObservable<T> BeforeItemsAdded => _list.BeforeItemsAdded;

            public IObservable<T> ItemsRemoved => _list.ItemsRemoved;

            public IObservable<T> BeforeItemsRemoved => _list.BeforeItemsRemoved;

            public IObservable<IMoveInfo<T>> BeforeItemsMoved => _list.BeforeItemsMoved;

            public IObservable<IMoveInfo<T>> ItemsMoved => _list.ItemsMoved;

            public IObservable<NotifyCollectionChangedEventArgs> Changing => _list.Changing;

            public IObservable<NotifyCollectionChangedEventArgs> Changed => _list.Changed;

            public IObservable<int> CountChanging => _list.CountChanging;

            public IObservable<int> CountChanged => _list.CountChanged;

            public IObservable<bool> IsEmptyChanged => _list.IsEmptyChanged;

            public IObservable<Unit> ShouldReset => _list.ShouldReset;

            public IObservable<IReactivePropertyChangedEventArgs<T>> ItemChanging => _list.ItemChanging;

            public IObservable<IReactivePropertyChangedEventArgs<T>> ItemChanged => _list.ItemChanged;

            public bool ChangeTrackingEnabled
            {
                get => _list.ChangeTrackingEnabled;
                set => _list.ChangeTrackingEnabled = value;
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add => _list.CollectionChanged += value;
                remove => _list.CollectionChanged -= value;
            }

            public event NotifyCollectionChangedEventHandler CollectionChanging
            {
                add => _list.CollectionChanging += value;
                remove => _list.CollectionChanging -= value;
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add => _list.PropertyChanged += value;
                remove => _list.PropertyChanged -= value;
            }

            public event PropertyChangingEventHandler PropertyChanging
            {
                add => _list.PropertyChanging += value;
                remove => _list.PropertyChanging -= value;
            }

            public void RaisePropertyChanging(PropertyChangingEventArgs args)
            {
                _list.RaisePropertyChanging(args);
            }

            public void RaisePropertyChanged(PropertyChangedEventArgs args)
            {
                _list.RaisePropertyChanged(args);
            }

            public T this[int index] => _list[index];

            public bool IsEmpty => _list.IsEmpty;
        }
    }
}
