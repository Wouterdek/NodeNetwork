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
    }
}
