using System.Windows;
using ExampleCalculatorApp.ViewModels;
using Microsoft.Win32;
using ReactiveUI;

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

            this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.viewHost.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.ValueLabel, v => v.valueLabel.Content);
            this.OneWayBind(ViewModel, vm => vm.LoadFile, v => v.loadButton.Command);
            this.OneWayBind(ViewModel, vm => vm.SaveFile, v => v.saveButton.Command);

            this.WhenActivated(d => d(
                ViewModel.SelectFile.RegisterHandler(interaction =>
                {
                    FileDialog dialog;
                    if (interaction.Input)
                    {
                        dialog = new OpenFileDialog();
                    }
                    else
                    {
                        dialog = new SaveFileDialog();
                    }

                    dialog.FileName = "network.json";
                    dialog.AddExtension = true;
                    dialog.DereferenceLinks = true;
                    dialog.DefaultExt = "json";

                    interaction.SetOutput((dialog.ShowDialog() ?? false) ? dialog.FileName : null);
                })
            ));
        }
    }
}