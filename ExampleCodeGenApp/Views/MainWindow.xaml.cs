using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System;
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;
using System.Windows.Controls.Primitives;

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

			this.WhenActivated(d =>
			{
				this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.NetworkViewModel, v => v.network.ViewModel).DisposeWith(d);
				this.OneWayBind(ViewModel, vm => vm.NodeList, v => v.nodeList.ViewModel).DisposeWith(d);
				this.OneWayBind(ViewModel, vm => vm.CodePreview, v => v.codePreviewView.ViewModel).DisposeWith(d);
				this.OneWayBind(ViewModel, vm => vm.CodeSim, v => v.codeSimView.ViewModel).DisposeWith(d);

				this.BindCommand(ViewModel, vm => vm.NetworkViewModelBulider.AutoLayout, v => v.autoLayoutButton).DisposeWith(d);

				this.BindCommand(ViewModel, vm => vm.NetworkViewModelBulider.StartAutoLayoutLive, v => v.startAutoLayoutLiveButton).DisposeWith(d);
				this.WhenAnyObservable(v => v.ViewModel.NetworkViewModelBulider.StartAutoLayoutLive.IsExecuting)
					.Select((isRunning) => isRunning ? Visibility.Collapsed : Visibility.Visible)
					.BindTo(this, v => v.startAutoLayoutLiveButton.Visibility).DisposeWith(d);

				this.BindCommand(ViewModel, vm => vm.NetworkViewModelBulider.StopAutoLayoutLive, v => v.stopAutoLayoutLiveButton);
				this.WhenAnyObservable(v => v.ViewModel.NetworkViewModelBulider.StartAutoLayoutLive.IsExecuting)
					.Select((isRunning) => isRunning ? Visibility.Visible : Visibility.Collapsed)
					.BindTo(this, v => v.stopAutoLayoutLiveButton.Visibility).DisposeWith(d);

				// Save Network
				this.Events().Closing.Subscribe(_ => ViewModel.NetworkViewModelBulider.SuspensionDriver.SaveAll("C:\\Nodes\\Code\\")).DisposeWith(d);
				this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveNodeNetwork).DisposeWith(d);
				this.BindCommand(ViewModel, vm => vm.Load, v => v.LoadNodeNetwork).DisposeWith(d);
				this.OneWayBind(ViewModel, vm => vm.NetworkViewModelBulider.SuspensionDriver.Expressions, v => v.LoadList.ItemsSource).DisposeWith(d);
				LoadList.Events().SelectionChanged
				.Where(x => LoadList?.SelectedItem != null && LoadList.Items.Count > 0)
				.Select(x => LoadList.SelectedItem.ToString())
				.Subscribe(x => NodeNetworkName.Text = x).DisposeWith(d);
			});

			this.ViewModel = new MainViewModel();
		}
	}
}
