using DynamicData;
using ExampleShaderEditorApp.Model;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;
using ReactiveUI;
using System.Runtime.Serialization;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public class ShaderOutputNodeViewModel : ShaderNodeViewModel, IAmTheOutputViewModel
    {
        static ShaderOutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<ShaderOutputNodeViewModel>));
        }

        [DataMember] public ShaderNodeInputViewModel ColorInput { get; set; } = new ShaderNodeInputViewModel(typeof(Vec3));

        public ShaderOutputNodeViewModel()
        {
            this.Name = "Shader Output";
            this.Category = NodeCategory.Misc;
            this.CanBeRemovedByUser = false;

            ColorInput.Name = "Color";
            this.Inputs.Add(ColorInput);
        }
    }
}
