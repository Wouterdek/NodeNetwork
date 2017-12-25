using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            this.OneWayBind(ViewModel, vm => vm.NodeListViewModel, v => v.nodeList.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.networkView.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel, v => v.shaderPreviewView.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel.FragmentShaderSource, v => v.shaderSource.Text, source => string.Join("\n", source));

            this.WhenAnyValue(v => v.shaderPreviewView.ActualWidth).BindTo(this, v => v.shaderPreviewView.Height);
        }
    }
}
