using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ExampleShaderEditorApp.ViewModels;
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

        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NodeListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.NetworkViewModel, v => v.networkView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel, v => v.shaderPreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel.FragmentShaderSource, v => v.shaderSource.Text, source => string.Join("\n", source)).DisposeWith(d);

                this.WhenAnyValue(v => v.shaderPreviewView.ActualWidth).BindTo(this, v => v.shaderPreviewView.Height).DisposeWith(d);

                // Save Network
                this.Events().Closing.Subscribe(_ => ViewModel.NetworkViewModelBulider.SuspensionDriver.SaveAll("C:\\Nodes\\Shader\\")).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveNodeNetwork).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Load, v => v.LoadNodeNetwork).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.SuspensionDriver.Expressions, v => v.LoadList.ItemsSource).DisposeWith(d);
                LoadList.Events().SelectionChanged
                .Where(x => LoadList?.SelectedItem != null && LoadList.Items.Count > 0)
                .Select(x => LoadList.SelectedItem.ToString())
                .Subscribe(x => NodeNetworkName.Text = x).DisposeWith(d);
            });

            nodeList.CVS.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }
    }
}
