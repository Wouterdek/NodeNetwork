using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
{
    public partial class CodeGenPendingConnectionView : IViewFor<CodeGenPendingConnectionViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodeGenPendingConnectionViewModel), typeof(CodeGenPendingConnectionView), new PropertyMetadata(null));

        public CodeGenPendingConnectionViewModel ViewModel
        {
            get => (CodeGenPendingConnectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CodeGenPendingConnectionViewModel)value;
        }
        #endregion

        public CodeGenPendingConnectionView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                PendingConnectionView.ViewModel = this.ViewModel;
                d(Disposable.Create(() => PendingConnectionView.ViewModel = null));
            });
        }
    }
}
