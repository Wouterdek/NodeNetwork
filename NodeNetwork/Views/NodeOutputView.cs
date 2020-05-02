using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodeNetwork.ViewModels;
using ReactiveUI;
using Splat;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(EndpointHost), Type = typeof(ViewModelViewHost))]
    [TemplatePart(Name = nameof(EditorHost), Type = typeof(ViewModelViewHost))]
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(Icon), Type = typeof(Image))]
    public class NodeOutputView : Control, IViewFor<NodeOutputViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeOutputViewModel), typeof(NodeOutputView), new PropertyMetadata(null));

        public NodeOutputViewModel ViewModel
        {
            get => (NodeOutputViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeOutputViewModel)value;
        }
        #endregion
        
        private ViewModelViewHost EndpointHost { get; set; }
        private ViewModelViewHost EditorHost { get; set; }
        private TextBlock NameLabel { get; set; }
        private Image Icon { get; set; }

        private bool _isHeaderEmpty;

        public NodeOutputView()
        {
            DefaultStyleKey = typeof(NodeOutputView);

            SetupBindings();
        }

        private void SetupBindings()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameLabel.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Port, v => v.EndpointHost.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Port.IsVisible, v => v.EndpointHost.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Editor, v => v.EditorHost.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Icon, v => v.Icon.Source, img => img?.ToNative()).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.Name, v => v.ViewModel.Icon,
                        (name, icon) => String.IsNullOrEmpty(name) && icon == null)
                    .Subscribe(v =>
                    {
                        _isHeaderEmpty = v;
                        if (EditorHost != null)
                        {
                            Grid.SetRow(EditorHost, _isHeaderEmpty ? 0 : 1);
                        }
                    })
                    .DisposeWith(d);
            });
        }

        public override void OnApplyTemplate()
        {
            EndpointHost = GetTemplateChild(nameof(EndpointHost)) as ViewModelViewHost;
            EditorHost = GetTemplateChild(nameof(EditorHost)) as ViewModelViewHost;
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBlock;
            Icon = GetTemplateChild(nameof(Icon)) as Image;

            Grid.SetRow(EditorHost, _isHeaderEmpty ? 0 : 1);
        }
    }
}
