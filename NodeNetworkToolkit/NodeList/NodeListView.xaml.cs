﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DynamicData;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.NodeList
{
    [DataContract]
    public partial class NodeListView : IViewFor<NodeListViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeListViewModel), typeof(NodeListView), new PropertyMetadata(null));

        [DataMember]
        public NodeListViewModel ViewModel
        {
            get => (NodeListViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [DataMember]
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeListViewModel)value;
        }
        #endregion

        #region Show/Hide properties
        public static readonly DependencyProperty ShowSearchProperty =
            DependencyProperty.Register(nameof(ShowSearch), typeof(bool), typeof(NodeListView), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowDisplayModeSelectorProperty =
            DependencyProperty.Register(nameof(ShowDisplayModeSelector), typeof(bool), typeof(NodeListView), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowTitleProperty =
            DependencyProperty.Register(nameof(ShowTitle), typeof(bool), typeof(NodeListView), new PropertyMetadata(true));

        [DataMember]
        public bool ShowSearch
        {
            get { return (bool)GetValue(ShowSearchProperty); }
            set { SetValue(ShowSearchProperty, value); }
        }

        [DataMember]
        public bool ShowDisplayModeSelector
        {
            get { return (bool)GetValue(ShowDisplayModeSelectorProperty); }
            set { SetValue(ShowDisplayModeSelectorProperty, value); }
        }

        [DataMember]
        public bool ShowTitle
        {
            get { return (bool)GetValue(ShowTitleProperty); }
            set { SetValue(ShowTitleProperty, value); }
        }
        #endregion

        [DataMember] public CollectionViewSource CVS { get; set; } = new CollectionViewSource();

        /// <summary>
        /// Gets a value indicating whether this instance is in design mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in design mode; otherwise, <c>false</c>.
        /// </value>
        [IgnoreDataMember] protected bool InDesignMode => DesignerProperties.GetIsInDesignMode(this);

        public NodeListView()
        {
            InitializeComponent();
            if (InDesignMode) { return; }

            viewComboBox.ItemsSource = Enum.GetValues(typeof(NodeListViewModel.DisplayMode)).Cast<NodeListViewModel.DisplayMode>();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Display, v => v.viewComboBox.SelectedItem).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.ItemTemplate,
                    displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles
                        ? Resources["tilesTemplate"]
                        : Resources["listTemplate"])
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.ItemsPanel,
                    displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles
                        ? Resources["tilesItemsPanelTemplate"]
                        : Resources["listItemsPanelTemplate"])
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Display, v => v.elementsList.Template,
                    displayMode => displayMode == NodeListViewModel.DisplayMode.Tiles
                        ? Resources["tilesItemsControlTemplate"]
                        : Resources["listItemsControlTemplate"])
                    .DisposeWith(d);

                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.searchBox.Text).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.VisibleNodes).Switch().Bind(out var bindableList).Subscribe().DisposeWith(d);
                CVS.Source = bindableList;
                elementsList.ItemsSource = CVS.View;

                this.WhenAnyObservable(v => v.ViewModel.VisibleNodes.CountChanged)
                    .Select(count => count == 0)
                    .BindTo(this, v => v.emptyMessage.Visibility).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Title, v => v.titleLabel.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EmptyLabel, v => v.emptyMessage.Text).DisposeWith(d);

                this.WhenAnyValue(v => v.searchBox.IsFocused, v => v.searchBox.Text)
                    .Select(t => !t.Item1 && string.IsNullOrWhiteSpace(t.Item2))
                    .BindTo(this, v => v.emptySearchBoxMessage.Visibility)
                    .DisposeWith(d);

                this.WhenAnyValue(v => v.ShowSearch)
                    .BindTo(this, v => v.searchBoxGrid.Visibility).DisposeWith(d);
                this.WhenAnyValue(v => v.ShowDisplayModeSelector)
                    .BindTo(this, v => v.viewComboBox.Visibility).DisposeWith(d);
                this.WhenAnyValue(v => v.ShowTitle)
                    .BindTo(this, v => v.titleLabel.Visibility).DisposeWith(d);
            });
        }

        private void OnNodeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                NodeViewModel nodeVM = ((FrameworkElement)sender).DataContext as NodeViewModel;
                if (nodeVM == null)
                {
                    return;
                }

                NodeViewModel newNodeVM = ViewModel.NodeFactories[nodeVM]();

                DragDrop.DoDragDrop(this, new DataObject("nodeVM", newNodeVM), DragDropEffects.Copy);
            }
        }
    }
}
