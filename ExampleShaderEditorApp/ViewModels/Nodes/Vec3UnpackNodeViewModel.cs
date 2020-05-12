using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using ExampleShaderEditorApp.Model;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public class Vec3UnpackNodeViewModel : ShaderNodeViewModel
    {
        static Vec3UnpackNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec3UnpackNodeViewModel>));
        }

        [DataMember] public ShaderNodeInputViewModel VectorInput { get; set; } = new ShaderNodeInputViewModel(typeof(Vec3));

        [DataMember] public ShaderNodeOutputViewModel X { get; set; } = new ShaderNodeOutputViewModel();
        [DataMember] public ShaderNodeOutputViewModel Y { get; set; } = new ShaderNodeOutputViewModel();
        [DataMember] public ShaderNodeOutputViewModel Z { get; set; } = new ShaderNodeOutputViewModel();

        public Vec3UnpackNodeViewModel()
        {
            this.Name = "Unpack Vec3";
            this.Category = NodeCategory.Vector;

            VectorInput.Name = "Vec3";
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

            Z.Name = "Z";
            Z.ReturnType = typeof(float);
            Z.Value = this.WhenAnyValue(vm => vm.VectorInput.Value).Select(v => v == null ? null : new ShaderFunc(() => $"({v.Compile()}).z"));
            Outputs.Add(Z);
        }
    }
}
