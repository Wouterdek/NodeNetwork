using DynamicData;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace NodeNetwork.Toolkit.ContextMenu
{
    /// <summary>
    /// A data type containing a command, parameter and display properties.
    /// </summary>
    public class LabeledCommand : ReactiveObject
    {
        #region Label
        /// <summary>
        /// The label that is displayed in the menu
        /// </summary>
        public string Label
        {
            get => _label;
            set => this.RaiseAndSetIfChanged(ref _label, value);
        }
        private string _label = "";
        #endregion

        #region Visible
        /// <summary>
        /// Should the command be displayed in the menu?
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set => this.RaiseAndSetIfChanged(ref _visible, value);
        }
        private bool _visible = true;
        #endregion

        #region Command
        /// <summary>
        /// The command to be executed.
        /// </summary>
        public ICommand Command
        {
            get => _command;
            set => this.RaiseAndSetIfChanged(ref _command, value);
        }
        private ICommand _command = null;
        #endregion

        #region CommandParameter
        /// <summary>
        /// The parameter to be passed to the command on execution.
        /// </summary>
        public object CommandParameter
        {
            get => _commandParameter;
            set => this.RaiseAndSetIfChanged(ref _commandParameter, value);
        }
        private object _commandParameter = null;
        #endregion
    }

    /// <summary>
    /// A viewmodel for a context menu in which the entries can be filtered by the user based on a searchquery.
    /// </summary>
    public class SearchableContextMenuViewModel : ReactiveObject
    {
        static SearchableContextMenuViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new SearchableContextMenuView(), typeof(IViewFor<SearchableContextMenuViewModel>));
        }

        /// <summary>
        /// List of all the available commands in the menu.
        /// </summary>
        public ISourceList<LabeledCommand> Commands { get; } = new SourceList<LabeledCommand>();

        /// <summary>
        /// List of commands that are actually visible in the menu.
        /// This list is based on Commands and SearchQuery.
        /// </summary>
        public IObservableList<LabeledCommand> VisibleCommands { get; }

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

        #region MaxItemsDisplayed
        /// <summary>
        /// Only the first MaxItemsDisplayed items from Commands that match the query are displayed.
        /// </summary>
        public int MaxItemsDisplayed
        {
            get => _maxItemsDisplayed;
            set => this.RaiseAndSetIfChanged(ref _maxItemsDisplayed, value);
        }
        private int _maxItemsDisplayed = int.MaxValue;
        #endregion

        public SearchableContextMenuViewModel()
        {
            var onQueryChanged = 
                this.WhenAnyValue(vm => vm.SearchQuery, vm => vm.MaxItemsDisplayed)
                .Throttle(TimeSpan.FromMilliseconds(70), RxApp.MainThreadScheduler)
                .Publish();
            onQueryChanged.Connect();

            VisibleCommands = Commands.Connect()
                .AutoRefreshOnObservable(_ => onQueryChanged)
                .AutoRefresh(cmd => cmd.Label)
                .AutoRefresh(cmd => cmd.Visible)
                .Filter(cmd => cmd.Visible && (cmd.Label ?? "").ToUpper().Contains(SearchQuery?.ToUpper() ?? ""))
                .Top(MaxItemsDisplayed)
                .AsObservableList();
        }
    }
}