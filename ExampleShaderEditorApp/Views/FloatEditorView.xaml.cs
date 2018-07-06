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
using ExampleShaderEditorApp.ViewModels.Editors;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class FloatEditorView : IViewFor<FloatEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(FloatEditorViewModel), typeof(FloatEditorView), new PropertyMetadata(null));

        public FloatEditorViewModel ViewModel
        {
            get => (FloatEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FloatEditorViewModel)value;
        }
        #endregion

        public FloatEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.FloatValue, v => v.upDown.Value)
            ));
        }
    }
}
