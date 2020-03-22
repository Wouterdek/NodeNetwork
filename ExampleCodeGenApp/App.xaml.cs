using System.Windows;
using ExampleCodeGenApp.ViewModels;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.ViewModels.Nodes;
using ExampleCodeGenApp.Views;
using ExampleCodeGenApp.Views.Editors;
using NodeNetwork.Utilities;
using ReactiveUI;
using Splat;

namespace ExampleCodeGenApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitSplatContainer();

            base.OnStartup(e);
        }

        private void InitSplatContainer()
        {
            var container = Locator.CurrentMutable;

            // Register NodeNetwork components. 
            container.UseNodeNetwork();
            
            // Register custom components. 
            container.Register(() => new CodeGenConnectionView(), typeof(IViewFor<CodeGenConnectionViewModel>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<CodeGenNodeViewModel>));
            container.Register(() => new CodeGenPendingConnectionView(), typeof(IViewFor<CodeGenPendingConnectionViewModel>));
            container.Register(() => new CodeGenPortView(), typeof(IViewFor<CodeGenPortViewModel>));
            container.Register(() => new IntegerValueEditorView(), typeof(IViewFor<IntegerValueEditorViewModel>));
            container.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<ButtonEventNode>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<ForLoopNode>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<IntLiteralNode>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<PrintNode>));
            container.Register(() => new CodeGenNodeView(), typeof(IViewFor<TextLiteralNode>));

            // ToDo - How do you add open generics to splat container? 
            /* container.Register(() => new NodeInputView(), typeof(IViewFor<CodeGenInputViewModel<>>));
            container.Register(() => new NodeInputView(), typeof(IViewFor<CodeGenListInputViewModel<>>));
            container.Register(() => new NodeOutputView(), typeof(IViewFor<CodeGenOutputViewModel<>>));*/ 
        }
    }
}
