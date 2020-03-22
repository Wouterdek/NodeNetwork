using System.Windows;
using ExampleShaderEditorApp.ViewModels;
using ExampleShaderEditorApp.ViewModels.Editors;
using ExampleShaderEditorApp.ViewModels.Nodes;
using ExampleShaderEditorApp.Views;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace ExampleShaderEditorApp
{
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
            container.Register(() => new ColorEditorView(), typeof(IViewFor<ColorEditorViewModel>));
            container.Register(() => new FloatEditorView(), typeof(IViewFor<FloatEditorViewModel>));
            container.Register(() => new Vec2EditorView(), typeof(ReactiveUI.IViewFor<Vec2EditorViewModel>));
            container.Register(() => new Vec3EditorView(), typeof(ReactiveUI.IViewFor<Vec3EditorViewModel>));
            container.Register(() => new EnumEditorView(), typeof(IViewFor<EnumEditorViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<ColorNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<GeometryNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Math2NodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<MathNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<ShaderOutputNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Vec2PackNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Vec2UnpackNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Vec3MathNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Vec3PackNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<Vec3UnpackNodeViewModel>));
            container.Register(() => new NodeInputView(), typeof(IViewFor<ShaderNodeInputViewModel>));
            container.Register(() => new NodeOutputView(), typeof(IViewFor<ShaderNodeOutputViewModel>));
        }
    }
}
