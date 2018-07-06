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
using ExampleShaderEditorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class ColorEditorView : IViewFor<ColorEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ColorEditorViewModel), typeof(ColorEditorView), new PropertyMetadata(null));

        public ColorEditorViewModel ViewModel
        {
            get => (ColorEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ColorEditorViewModel)value;
        }
        #endregion

        public ColorEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.ColorValue, v => v.colorPicker.SelectedColor)
            ));
        }
    }
}
