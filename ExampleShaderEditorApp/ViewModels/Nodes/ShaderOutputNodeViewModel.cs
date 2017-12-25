using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleShaderEditorApp.Model;
using NodeNetwork;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class ShaderOutputNodeViewModel : NodeViewModel
    {
        static ShaderOutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<ShaderOutputNodeViewModel>));
        }

        public ShaderNodeInputViewModel ColorInput { get; } = new ShaderNodeInputViewModel(typeof(Vec3));

        public ShaderOutputNodeViewModel()
        {
            this.Name = "Shader Output";
            this.CanBeRemovedByUser = false;

            ColorInput.Name = "Color";
            this.Inputs.Add(ColorInput);
        }
    }
}
