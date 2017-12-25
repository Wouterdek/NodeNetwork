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
    public partial class NodeEndpointEditorView : IViewFor<NodeEndpointEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeEndpointEditorViewModel), typeof(NodeEndpointEditorView), new PropertyMetadata(null));

        public NodeEndpointEditorViewModel ViewModel
        {
            get => (NodeEndpointEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeEndpointEditorViewModel)value;
        }
        #endregion

        public NodeEndpointEditorView()
        {
            InitializeComponent();
        }
    }
}
