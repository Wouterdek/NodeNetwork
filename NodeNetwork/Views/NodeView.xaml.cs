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
    public partial class NodeView : IViewFor<NodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeViewModel), typeof(NodeView), new PropertyMetadata(null));

        public NodeViewModel ViewModel
        {
            get => (NodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeViewModel)value;
        }
        #endregion

        public NodeView()
        {
            InitializeComponent();

            this.Bind(ViewModel, vm => vm.IsCollapsed, v => v.collapseButton.IsChecked);

            this.OneWayBind(ViewModel, vm => vm.Name, v => v.nameLabel.Text);

            this.OneWayBind(ViewModel, vm => vm.VisibleInputs, v => v.inputsList.ItemsSource);
            this.OneWayBind(ViewModel, vm => vm.VisibleOutputs, v => v.outputsList.ItemsSource);
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            this.Focus();

            if (ViewModel.IsSelected)
            {
                return;
            }

            if (ViewModel.Parent != null && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ViewModel.Parent.ClearSelection();
            }
            ViewModel.IsSelected = true;
        }
    }
}
