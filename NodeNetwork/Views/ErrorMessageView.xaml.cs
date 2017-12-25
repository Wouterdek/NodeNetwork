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
    public partial class ErrorMessageView : IViewFor<ErrorMessageViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ErrorMessageViewModel), typeof(ErrorMessageView), new PropertyMetadata(null));

        public ErrorMessageViewModel ViewModel
        {
            get => (ErrorMessageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ErrorMessageViewModel)value;
        }
        #endregion

        public ErrorMessageView()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.Message, v => v.textBlock.Text);
        }
    }
}
