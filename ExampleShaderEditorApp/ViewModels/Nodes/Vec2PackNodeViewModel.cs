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
    public class Vec2PackNodeViewModel : ShaderNodeViewModel
    {
        static Vec2PackNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec2PackNodeViewModel>));
        }

        [DataMember] public ShaderNodeInputViewModel XInput { get; set; } = new ShaderNodeInputViewModel(typeof(float));
        [DataMember] public ShaderNodeInputViewModel YInput { get; set; } = new ShaderNodeInputViewModel(typeof(float));

        [DataMember] public ShaderNodeOutputViewModel Result { get; set; } = new ShaderNodeOutputViewModel();

        public Vec2PackNodeViewModel()
        {
            this.Name = "New Vec2";
            this.Category = NodeCategory.Vector;

            XInput.Name = "X";
            XInput.Editor = new FloatEditorViewModel();
            Inputs.Add(XInput);

            YInput.Name = "Y";
            YInput.Editor = new FloatEditorViewModel();
            Inputs.Add(YInput);

            Result.Name = "Vec2";
            Result.ReturnType = typeof(Vec2);
            Result.Value = this.WhenAnyValue(vm => vm.XInput.Value, vm => vm.YInput.Value)
                .Select(t =>
                {
                    if (t.Item1 == null || t.Item2 == null)
                    {
                        return null;
                    }
                    return new ShaderFunc(() => $"vec2(({t.Item1.Compile()}), ({t.Item2.Compile()}))");
                });
            Outputs.Add(Result);
        }
    }
}
