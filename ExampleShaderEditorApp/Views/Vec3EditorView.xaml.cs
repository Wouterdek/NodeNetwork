using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.ViewModels.Editors;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class Vec3EditorView : IViewFor<Vec3EditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(Vec3EditorViewModel), typeof(Vec3EditorView), new PropertyMetadata(null));

        public Vec3EditorViewModel ViewModel
        {
            get => (Vec3EditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (Vec3EditorViewModel)value;
        }
        #endregion

        public Vec3EditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.xUpDown.Value, v => v.yUpDown.Value, v => v.zUpDown.Value)
                    .Select(c => new Vec3(c.Item1 ?? 0, c.Item2 ?? 0, c.Item3 ?? 0))
                    .BindTo(this, v => v.ViewModel.Vec3Value)
                    .DisposeWith(d);
            });
        }
    }
}
