using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.NodeList
{
    public class NodeListViewModel : ReactiveObject
    {
        static NodeListViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeListView(), typeof(IViewFor<NodeListViewModel>));
        }

        public enum DisplayMode
        {
            Tiles, List
        }

        #region Title
        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        #endregion
        
        #region EmptyLabel
        private string _emptyLabel;
        public string EmptyLabel
        {
            get => _emptyLabel;
            set => this.RaiseAndSetIfChanged(ref _emptyLabel, value);
        }
        #endregion

        #region DisplayMode
        private DisplayMode _display;
        public DisplayMode Display
        {
            get => _display;
            set => this.RaiseAndSetIfChanged(ref _display, value);
        }
        #endregion

        public Dictionary<Type, Func<NodeViewModel>> NodeFactories { get; } = new Dictionary<Type, Func<NodeViewModel>>();
        public ReactiveList<NodeViewModel> Nodes { get; } = new ReactiveList<NodeViewModel>();
        public ReactiveList<NodeViewModel> VisibleNodes { get; } = new ReactiveList<NodeViewModel>();

        #region SearchQuery
        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }
        #endregion
        
        public NodeListViewModel()
        {
            Title = "Add node";
            EmptyLabel = "No matching nodes found.";
            Display = DisplayMode.Tiles;

            Observable.CombineLatest(this.WhenAnyValue(vm => vm.SearchQuery), this.WhenAnyObservable(vm => vm.Nodes.Changed), (a, b) => Unit.Default)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
                .Select(_ => Nodes.Where(n => (n.Name ?? "").ToUpper().Contains(SearchQuery?.ToUpper() ?? "")).ToList())
                .BindListContents(this, vm => vm.VisibleNodes);
        }

        public void AddNodeType<T>(Func<T> factory) where T : NodeViewModel
        {
            NodeFactories.Add(typeof(T), factory);
            Nodes.Add(factory());
        }
    }
}
