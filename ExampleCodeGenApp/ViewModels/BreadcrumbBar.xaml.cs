using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using DynamicData;
using NodeNetwork.Utilities;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace ExampleCodeGenApp.ViewModels
{
    public class BreadcrumbBarViewModel : ReactiveObject
    {
        public ISourceList<BreadcrumbViewModel> ActivePath { get; } = new SourceList<BreadcrumbViewModel>();

        private readonly ObservableAsPropertyHelper<BreadcrumbViewModel> _activeItem;
        public BreadcrumbViewModel ActiveItem => _activeItem.Value;

        public ReactiveCommand<BreadcrumbViewModel, Unit> SelectCrumb { get; }

        public BreadcrumbBarViewModel()
        {
            SelectCrumb = ReactiveCommand.Create((BreadcrumbViewModel crumb) =>
            {
                ActivePath.Edit(l =>
                {
                    int index = l.IndexOf(crumb);
                    for (int i = l.Count - 1; i > index; i--)
                    {
                        l.RemoveAt(i);
                    }
                });
            });

            ActivePath.Connect().Select(_ => ActivePath.Count > 0 ? ActivePath.Items.ElementAt(ActivePath.Count - 1) : null)
                .ToProperty(this, vm => vm.ActiveItem, out _activeItem);
        }
    }

    public class BreadcrumbViewModel : ReactiveObject
    {
        #region Name
        private string _name = "";
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        #endregion
    }

    public partial class BreadcrumbBar : UserControl, IViewFor<BreadcrumbBarViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(BreadcrumbBarViewModel), typeof(BreadcrumbBar), new PropertyMetadata(null));

        public BreadcrumbBarViewModel ViewModel
        {
            get => (BreadcrumbBarViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BreadcrumbBarViewModel)value;
        }
        #endregion

        public BreadcrumbBar()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.BindList(ViewModel, vm => vm.ActivePath, v => v.list.ItemsSource).DisposeWith(d);
                this.WhenAnyValue(v => v.list.SelectedItem)
                    .Where(i => i != null)
                    .Cast<BreadcrumbViewModel>()
                    .Do(_ => list.UnselectAll())
                    .InvokeCommand(this, v => v.ViewModel.SelectCrumb).DisposeWith(d);
            });
        }
    }
}
