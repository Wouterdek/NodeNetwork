using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [DataContract]
    public class NodeEndpointEditorView : Control, IViewFor<NodeEndpointEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeEndpointEditorViewModel), typeof(NodeEndpointEditorView), new PropertyMetadata(null));

        [DataMember]
        public NodeEndpointEditorViewModel ViewModel
        {
            get => (NodeEndpointEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [DataMember]
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeEndpointEditorViewModel)value;
        }
        #endregion

        public NodeEndpointEditorView()
        {
            DefaultStyleKey = typeof(NodeEndpointEditorView);
        }
    }
}
