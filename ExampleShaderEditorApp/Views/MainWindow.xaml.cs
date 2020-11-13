using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ExampleShaderEditorApp.ViewModels;
using NodeNetwork.Toolkit.ContextMenu;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        private readonly SearchableContextMenuView pendingConnectionContextMenuView = new SearchableContextMenuView();

        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NodeListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.networkView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel, v => v.shaderPreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel.FragmentShaderSource, v => v.shaderSource.Text, source => string.Join("\n", source)).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NetworkViewModel.ZoomFactor, v => v.zoomFactorSlider.Value).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel.MaxZoomLevel, v => v.zoomFactorSlider.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel.MinZoomLevel, v => v.zoomFactorSlider.Minimum).DisposeWith(d);
                this.viewAllButton.Events().Click.Subscribe(x => networkView.CenterAndZoomView()).DisposeWith(d);

                this.WhenAnyValue(v => v.shaderPreviewView.ActualWidth).BindTo(this, v => v.shaderPreviewView.Height).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.CollapseAllCommand, v => v.collapseAllMenuItem).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.AddNodeMenuVM, v => v.contextMenuView.ViewModel).DisposeWith(d);

                // Place nodes at OpenPoint, the point where the menu was opened
                ViewModel.AddNodeMenuVM.NodePositionFunc = n => contextMenuView.OpenPoint;
                ViewModel.AddNodeForPendingConnectionMenuVM.NodePositionFunc = n => pendingConnectionContextMenuView.OpenPoint;

                // Calculate OpenPoint relative to the canvas in which the nodes lie. (See NodePositionFunc above)
                contextMenuView.ReferencePointElement = networkView.CanvasOriginElement;
                pendingConnectionContextMenuView.ReferencePointElement = networkView.CanvasOriginElement;

                ViewModel.AddNodeForPendingConnectionMenuVM.OpenContextMenu.RegisterHandler(ctx =>
                {
                    var pendingConMenuVm = ctx.Input;
                    pendingConnectionContextMenuView.ViewModel = pendingConMenuVm;
                    pendingConnectionContextMenuView.IsOpen = true;
                    ctx.SetOutput(Unit.Default);
                }).DisposeWith(d);
            });

            nodeList.CVS.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }
    }
}
