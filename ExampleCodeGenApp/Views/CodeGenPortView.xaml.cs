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
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public partial class CodeGenPortView : IViewFor<CodeGenPortViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodeGenPortViewModel), typeof(CodeGenPortView), new PropertyMetadata(null));

        public CodeGenPortViewModel ViewModel
        {
            get => (CodeGenPortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CodeGenPortViewModel)value;
        }
        #endregion

        #region Template Resource Keys
        public const String ExecutionPortTemplateKey = "ExecutionPortTemplate";
        public const String IntegerPortTemplateKey = "IntegerPortTemplate";
        public const String StringPortTemplateKey = "StringPortTemplate"; 
        #endregion

        public CodeGenPortView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                PortView.ViewModel = this.ViewModel;
                d(Disposable.Create(() => PortView.ViewModel = null));
            });

            this.OneWayBind(ViewModel, vm => vm.PortType, v => v.PortView.Template, GetTemplateFromPortType);

            this.OneWayBind(ViewModel, vm => vm.IsMirrored, v => v.PortView.RenderTransform, isMirrored => new ScaleTransform(isMirrored ? -1.0 : 1.0, 1.0));
        }

        public ControlTemplate GetTemplateFromPortType(PortType type)
        {
            switch (type)
            {
                case PortType.Execution: return (ControlTemplate) Resources[ExecutionPortTemplateKey];
                case PortType.Integer: return (ControlTemplate) Resources[IntegerPortTemplateKey];
                case PortType.String: return (ControlTemplate) Resources[StringPortTemplateKey];
                default: throw new Exception("Unsupported port type");
            }
        }
    }
}
