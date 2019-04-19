using System;
using System.Reactive.Disposables;
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
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.networkView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel, v => v.shaderPreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ShaderPreviewViewModel.FragmentShaderSource, v => v.shaderSource.Text, source => string.Join("\n", source)).DisposeWith(d);

                this.WhenAnyValue(v => v.shaderPreviewView.ActualWidth).BindTo(this, v => v.shaderPreviewView.Height).DisposeWith(d);
            });

            nodeList.CVS.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }
    }
}
