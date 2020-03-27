﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using ExampleCodeGenApp.ViewModels;
using ExampleCodeGenPrismApp.Views;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
{
    public partial class MainWindow : WindowViewBase<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Network, v => v.network.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NodeList, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodePreview, v => v.codePreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim, v => v.codeSimView.ViewModel).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.AutoLayout, v => v.autoLayoutButton);

                this.BindCommand(ViewModel, vm => vm.StartAutoLayoutLive, v => v.startAutoLayoutLiveButton);
                this.WhenAnyObservable(v => v.ViewModel.StartAutoLayoutLive.IsExecuting)
	                .Select((isRunning) => isRunning ? Visibility.Collapsed : Visibility.Visible)
	                .BindTo(this, v => v.startAutoLayoutLiveButton.Visibility);

                this.BindCommand(ViewModel, vm => vm.StopAutoLayoutLive, v => v.stopAutoLayoutLiveButton);
				this.WhenAnyObservable(v => v.ViewModel.StartAutoLayoutLive.IsExecuting)
					.Select((isRunning) => isRunning ? Visibility.Visible : Visibility.Collapsed)
					.BindTo(this, v => v.stopAutoLayoutLiveButton.Visibility);
			});
        }
    }
}
