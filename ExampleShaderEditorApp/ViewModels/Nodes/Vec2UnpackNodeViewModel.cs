using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.ViewModels.Editors;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class Vec2UnpackNodeViewModel : ShaderNodeViewModel
    {
        static Vec2UnpackNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec2UnpackNodeViewModel>));
        }

        public ShaderNodeInputViewModel VectorInput { get; } = new ShaderNodeInputViewModel(typeof(Vec2));

        public ShaderNodeOutputViewModel X { get; } = new ShaderNodeOutputViewModel();
        public ShaderNodeOutputViewModel Y { get; } = new ShaderNodeOutputViewModel();

        public Vec2UnpackNodeViewModel()
        {
            this.Name = "Unpack Vec2";
            this.Category = NodeCategory.Vector;

            VectorInput.Name = "Vec2";
            VectorInput.Editor = null;
            Inputs.Add(VectorInput);

            X.Name = "X";
            X.ReturnType = typeof(float);
            X.Value = this.WhenAnyValue(vm => vm.VectorInput.Value).Select(v => v == null ? null : new ShaderFunc(() => $"({v.Compile()}).x"));
            Outputs.Add(X);

            Y.Name = "Y";
            Y.ReturnType = typeof(float);
            Y.Value = this.WhenAnyValue(vm => vm.VectorInput.Value).Select(v => v == null ? null : new ShaderFunc(() => $"({v.Compile()}).y"));
            Outputs.Add(Y);
        }
    }
}
