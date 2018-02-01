using System.Windows;
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
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

            this.OneWayBind(ViewModel, vm => vm.Network, v => v.network.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.NodeList, v => v.nodeList.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.CodePreview, v => v.codePreviewView.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.CodeSim, v => v.codeSimView.ViewModel);

            this.ViewModel = new MainViewModel();
        }
    }
}
