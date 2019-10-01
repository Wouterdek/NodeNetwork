using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace NodeNetwork.Utilities
{
    public static class ReactiveExtensions
    {
        /// <summary>
        /// Pass through values if and only if the last value produced by 'throttleCondition' is false.
        /// When 'throttleCondition' is false, no values are passed through.
        /// When 'throttleCondition' changes from true to false, if one or more values was blocked during the period
        /// in which the throttle was active, the latest value will be passed through.
        /// </summary>
        /// <typeparam name="T">The datatype in the observable</typeparam>
        /// <param name="self">The source observable</param>
        /// <param name="throttleCondition">An observable of booleans that determines the current throttle state</param>
        /// <returns>The new observable</returns>
        public static IObservable<T> ThrottleWhen<T>(this IObservable<T> self, IObservable<bool> throttleCondition)
        {
            var isPaused = throttleCondition.Prepend(false).DistinctUntilChanged();
            return Observable.Defer(() =>
            {
                object lockObj = new object();
                bool gateIsOpen = false;
                return Observable.CombineLatest(
                        self.Synchronize(lockObj).Do(_ => gateIsOpen = true),
                        isPaused.Synchronize(lockObj).Do(paused => gateIsOpen = !paused && gateIsOpen),
                        (number, paused) => (number, paused)
                    )
                    .Where(tuple => !tuple.paused && gateIsOpen)
                    .Select(tuple => tuple.number);
            });
        }

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
    }
}
