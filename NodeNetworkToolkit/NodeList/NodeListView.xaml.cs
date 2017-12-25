using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.NodeList
{
    public partial class NodeListView : IViewFor<NodeListViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeListViewModel), typeof(NodeListView), new PropertyMetadata(null));

        public NodeListViewModel ViewModel
        {
            get => (NodeListViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeListViewModel)value;
        }
        #endregion
        
        public NodeListView()
        {
            InitializeComponent();

            viewComboBox.ItemsSource = Enum.GetValues(typeof(NodeListViewModel.DisplayMode)).Cast<NodeListViewModel.DisplayMode>();
            this.Bind(ViewModel, vm => vm.Display, v => v.viewComboBox.SelectedItem);

            this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.ItemTemplate,
                displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles ? Resources["tilesTemplate"] : Resources["listTemplate"]);

            this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.ItemsPanel,
                displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles ? Resources["tilesItemsPanelTemplate"] : Resources["listItemsPanelTemplate"]);

            this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.Template,
                displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles ? Resources["tilesItemsControlTemplate"] : Resources["listItemsControlTemplate"]);

            this.Bind(ViewModel, vm => vm.SearchQuery, v => v.searchBox.Text);
            this.OneWayBind(ViewModel, vm => vm.VisibleNodes, v => v.elementsList.ItemsSource);
            this.OneWayBind(ViewModel, vm => vm.VisibleNodes.IsEmpty, v => v.emptyMessage.Visibility);

            this.OneWayBind(ViewModel, vm => vm.Title, v => v.titleLabel.Content);
            this.OneWayBind(ViewModel, vm => vm.EmptyLabel, v => v.emptyMessage.Text);

            this.WhenAnyValue(v => v.searchBox.IsFocused, v => v.searchBox.Text)
                .Select(t => !t.Item1 && string.IsNullOrWhiteSpace(t.Item2))
                .BindTo(this, v => v.emptySearchBoxMessage.Visibility);
        }

        private void OnNodeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                NodeViewModel nodeVM = ((FrameworkElement)sender).DataContext as NodeViewModel;

                NodeViewModel newNodeVM = ViewModel.NodeFactories[nodeVM.GetType()]();

                DragDrop.DoDragDrop(this, new DataObject("nodeVM", newNodeVM), DragDropEffects.Copy);
            }
        }
    }
}
