using System.Reactive.Disposables;
using System.Windows;
using ExampleCalculatorApp.ViewModels;
using ReactiveUI;
using System;
using System.Windows.Controls.Primitives;
using System.Reactive.Linq;

namespace ExampleCalculatorApp.Views
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
                this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.NetworkViewModel, v => v.viewHost.ViewModel).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveNodeNetwork).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Load, v => v.LoadNodeNetwork).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ValueLabel, v => v.valueLabel.Content).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.SuspensionDriver.Expressions, v => v.LoadList.ItemsSource).DisposeWith(d);
                this.Events().Closing.Subscribe(_ => ViewModel.NetworkViewModelBulider.SuspensionDriver.SaveAll("C:\\Nodes\\Math\\")).DisposeWith(d);
                LoadList.Events().SelectionChanged
                .Where(x => LoadList?.SelectedItem != null && LoadList.Items.Count > 0)
                .Select(x=>LoadList.SelectedItem.ToString())
                .Subscribe(x=>NodeNetworkName.Text = x).DisposeWith(d);
            });
        }
    }
}