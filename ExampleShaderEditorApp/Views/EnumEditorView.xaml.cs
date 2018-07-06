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
using ExampleShaderEditorApp.ViewModels;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class EnumEditorView : IViewFor<EnumEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(EnumEditorViewModel), typeof(EnumEditorView), new PropertyMetadata(null));

        public EnumEditorViewModel ViewModel
        {
            get => (EnumEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (EnumEditorViewModel)value;
        }
        #endregion

        public EnumEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.OptionLabels, v => v.valueComboBox.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedOptionIndex, v => v.valueComboBox.SelectedIndex).DisposeWith(d);
            });
        }
    }
}
