using System;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
{
    [DataContract]
    public partial class CodeGenPortView : IViewFor<CodeGenPortViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodeGenPortViewModel), typeof(CodeGenPortView), new PropertyMetadata(null));

        [DataMember]
        public CodeGenPortViewModel ViewModel
        {
            get => (CodeGenPortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        [DataMember]
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
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.PortView.ViewModel).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.PortType, v => v.PortView.Template, GetTemplateFromPortType)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsMirrored, v => v.PortView.RenderTransform,
                    isMirrored => new ScaleTransform(isMirrored ? -1.0 : 1.0, 1.0))
                    .DisposeWith(d);
            });
        }

        public ControlTemplate GetTemplateFromPortType(PortType type)
        {
            return type switch
            {
                PortType.Execution => (ControlTemplate)Resources[ExecutionPortTemplateKey],
                PortType.Integer => (ControlTemplate)Resources[IntegerPortTemplateKey],
                PortType.String => (ControlTemplate)Resources[StringPortTemplateKey],
                _ => throw new Exception("Unsupported port type"),
            };
        }
    }
}
