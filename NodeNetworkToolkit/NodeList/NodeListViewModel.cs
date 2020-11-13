using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.NodeList
{
    /// <summary>
    /// A viewmodel for a UI List component that contains NodeViewModels
    /// and can be used to let the user add new nodes to a network.
    /// </summary>
    public class NodeListViewModel : ReactiveObject
    {
        static NodeListViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeListView(), typeof(IViewFor<NodeListViewModel>));
        }

        /// <summary>
        /// The formatting mode of the list.
        /// </summary>
        public enum DisplayMode
        {
            /// <summary>
            /// The nodes are displayed graphically in a grid.
            /// </summary>
            Tiles,
            /// <summary>
            /// The node names are displayed as text in a list.
            /// </summary>
            List
        }

        #region Title
        /// <summary>
        /// The string that is displayed at the top of the list
        /// </summary>
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        private string _title;
        #endregion

        #region EmptyLabel
        /// <summary>
        /// The string that is displayed when VisibleNodes is empty.
        /// </summary>
        public string EmptyLabel
        {
            get => _emptyLabel;
            set => this.RaiseAndSetIfChanged(ref _emptyLabel, value);
        }
        private string _emptyLabel = "";
        #endregion

        #region DisplayMode
        /// <summary>
        /// The way the list of available nodes is formatted.
        /// </summary>
        public DisplayMode Display
        {
            get => _display;
            set => this.RaiseAndSetIfChanged(ref _display, value);
        }
        private DisplayMode _display;
        #endregion

        #region NodeTemplates
        /// <summary>
        /// List of all the available nodes in the list.
        /// </summary>
        public ISourceList<NodeTemplate> NodeTemplates { get; } = new SourceList<NodeTemplate>();
        #endregion

        #region VisibleNodes
        /// <summary>
        /// List of nodes that are actually visible in the list.
        /// This list is based on Nodes and SearchQuery.
        /// </summary>
        public IObservableList<NodeViewModel> VisibleNodes { get; }
        #endregion

        #region SearchQuery
        /// <summary>
        /// The current search string that is used to filter Nodes into VisibleNodes.
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }
        private string _searchQuery = "";
        #endregion

        public NodeListViewModel()
        {
            Title = "Add node";
            EmptyLabel = "No matching nodes found.";
            Display = DisplayMode.Tiles;

	        var onQueryChanged = this.WhenAnyValue(vm => vm.SearchQuery)
		        .Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
		        .Publish();
	        onQueryChanged.Connect();
	        VisibleNodes = NodeTemplates.Connect()
		        .AutoRefreshOnObservable(_ => onQueryChanged)
                .Transform(t => t.Instance)
		        .AutoRefresh(node => node.Name)
		        .Filter(n => (n.Name ?? "").ToUpper().Contains(SearchQuery?.ToUpper() ?? ""))
		        .AsObservableList();
        }

        /// <summary>
        /// Adds a new node type to the list.
        /// Every time a node is added to a network from this list, the factory function will be called to create a new instance of the viewmodel type.
        /// </summary>
        /// <typeparam name="T">The subtype of NodeViewModel to add to the list.</typeparam>
        /// <param name="factory">The factory function to create a new instance of T</param>
        public void AddNodeType<T>(Func<T> factory) where T : NodeViewModel
        {
            NodeTemplates.Add(new NodeTemplate(factory));
        }
    }
}
