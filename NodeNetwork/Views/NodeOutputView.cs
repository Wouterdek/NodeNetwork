using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(EndpointHost), Type = typeof(ViewModelViewHost))]
    [TemplatePart(Name = nameof(EditorHost), Type = typeof(ViewModelViewHost))]
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBlock))]
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

        public NodeOutputView()
        {
            DefaultStyleKey = typeof(NodeOutputView);
        }

        private void SetupBindings()
        {
            this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameLabel.Text);
            this.OneWayBind(ViewModel, vm => vm.Port, v => v.EndpointHost.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.Port.IsVisible, v => v.EndpointHost.Visibility);
            this.OneWayBind(ViewModel, vm => vm.Editor, v => v.EditorHost.ViewModel);
        }

        public override void OnApplyTemplate()
        {
            EndpointHost = GetTemplateChild(nameof(EndpointHost)) as ViewModelViewHost;
            EditorHost = GetTemplateChild(nameof(EditorHost)) as ViewModelViewHost;
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBlock;

            SetupBindings();
        }
    }
}
