using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
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
    [DataContract]
    public class Vec2UnpackNodeViewModel : ShaderNodeViewModel
    {
        static Vec2UnpackNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec2UnpackNodeViewModel>));
        }

        [DataMember] public ShaderNodeInputViewModel VectorInput { get; set; } = new ShaderNodeInputViewModel(typeof(Vec2));

        [DataMember] public ShaderNodeOutputViewModel X { get; set; } = new ShaderNodeOutputViewModel();
        [DataMember] public ShaderNodeOutputViewModel Y { get; set; } = new ShaderNodeOutputViewModel();

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
