using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    public partial class NodeOutputView : IViewFor<NodeOutputViewModel>
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
        
        public NodeOutputView()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.Name, v => v.nameLabel.Text);
            this.OneWayBind(ViewModel, vm => vm.Port, v => v.endpointHost.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.Port.IsVisible, v => v.endpointHost.Visibility);
            this.OneWayBind(ViewModel, vm => vm.Editor, v => v.editorHost.ViewModel);
        }
    }
}
