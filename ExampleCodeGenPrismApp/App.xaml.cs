using System.Windows;
using ExampleCodeGenApp.ViewModels;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.ViewModels.Nodes;
using ExampleCodeGenApp.Views;
using ExampleCodeGenApp.Views.Editors;
using ExampleCodeGenPrismApp.Infrastructure;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;

namespace ExampleCodeGenApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var splatteredRegistry = Container.UseSplatteredPrismViewLocator();

            // Splat/ReactiveUI resolve Views from View Models
            // Register Node Network Elements
            splatteredRegistry.Register(typeof(ConnectionView), typeof(ConnectionViewModel));
            splatteredRegistry.Register(typeof(ErrorMessageView), typeof(ErrorMessageViewModel));
            splatteredRegistry.Register(typeof(NetworkView), typeof(NetworkViewModel));
            splatteredRegistry.Register(typeof(NodeEndpointEditorView), typeof(NodeEndpointEditorViewModel));
            splatteredRegistry.Register(typeof(NodeInputView), typeof(NodeInputViewModel));
            splatteredRegistry.Register(typeof(NodeOutputView), typeof(NodeOutputViewModel));
            splatteredRegistry.Register(typeof(NodeView), typeof(NodeViewModel));
            splatteredRegistry.Register(typeof(PendingConnectionView), typeof(PendingConnectionViewModel));
            splatteredRegistry.Register(typeof(PortView), typeof(PortViewModel));

            // Register custom Node Network Elements
            splatteredRegistry.Register(typeof(CodeGenConnectionView), typeof(CodeGenConnectionViewModel));
            splatteredRegistry.Register(typeof(NodeInputView), typeof(CodeGenInputViewModel<>));
            splatteredRegistry.Register(typeof(NodeInputView), typeof(CodeGenListInputViewModel<>));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(CodeGenNodeViewModel));
            splatteredRegistry.Register(typeof(NodeOutputView), typeof(CodeGenOutputViewModel<>));
            splatteredRegistry.Register(typeof(CodeGenPendingConnectionView), typeof(CodeGenPendingConnectionViewModel));
            splatteredRegistry.Register(typeof(CodeGenPortView), typeof(CodeGenPortViewModel));
            splatteredRegistry.Register(typeof(IntegerValueEditorView), typeof(IntegerValueEditorViewModel));
            splatteredRegistry.Register(typeof(StringValueEditorView), typeof(StringValueEditorViewModel));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(ButtonEventNode));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(ForLoopNode));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(IntLiteralNode));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(PrintNode));
            splatteredRegistry.Register(typeof(CodeGenNodeView), typeof(TextLiteralNode));

            // Prism resolves View Models from Views
            // Explicit registration required, because naming convention not followed. 
            ViewModelLocationProvider.Register<MainWindow, MainViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
    }
}
