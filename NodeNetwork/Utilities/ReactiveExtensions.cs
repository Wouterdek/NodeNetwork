using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// </summary>
        /// <typeparam name="TObj">The type of viewmodel</typeparam>
        /// <typeparam name="TListItem">The type of object contained in the list</typeparam>
        /// <param name="data">The observable to take lists from.</param>
        /// <param name="target">The viewmodel that is used as a base for finding the target list property</param>
        /// <param name="property">The IReactiveList property that will be modified.</param>
        /// <returns>A disposable to break the binding</returns>
        public static IDisposable BindListContents<TObj, TListItem>(this IObservable<IList<TListItem>> data, TObj target, Expression<Func<TObj, IReactiveList<TListItem>>> property)
        {
            IObservable<IReactiveList<TListItem>> targetListObservable = target.WhenAnyValue(property);

            return Observable.CombineLatest(targetListObservable, data).Subscribe(latest =>
            {
                IReactiveList<TListItem> latestTargetList = latest[0] as IReactiveList<TListItem>;
                IList<TListItem> latestData = latest[1];

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
                    foreach ((int index, TListItem item, LongestCommonSubsequence.ChangeType changeType) change in changes)
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
