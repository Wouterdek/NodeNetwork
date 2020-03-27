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
    public partial class CodeSimView : UserControl, IViewFor<CodeSimViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodeSimViewModel), typeof(CodeSimView), new PropertyMetadata(null));

        public CodeSimViewModel ViewModel
        {
            get => (CodeSimViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CodeSimViewModel)value;
        }
        #endregion

        public CodeSimView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.RunScript, v => v.runButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ClearOutput, v => v.clearButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Output, v => v.outputTextBlock.Text).DisposeWith(d);
            });
        }
    }
}
