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
    public class Vec3PackNodeViewModel : ShaderNodeViewModel
    {
        static Vec3PackNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec3PackNodeViewModel>));
        }

        public ShaderNodeInputViewModel XInput { get; } = new ShaderNodeInputViewModel(typeof(float));
        public ShaderNodeInputViewModel YInput { get; } = new ShaderNodeInputViewModel(typeof(float));
        public ShaderNodeInputViewModel ZInput { get; } = new ShaderNodeInputViewModel(typeof(float));

        public ShaderNodeOutputViewModel Result { get; } = new ShaderNodeOutputViewModel();

        public Vec3PackNodeViewModel()
        {
            this.Name = "New Vec3";
            this.Category = NodeCategory.Vector;

            XInput.Name = "X";
            XInput.Editor = new FloatEditorViewModel();
            Inputs.Add(XInput);

            YInput.Name = "Y";
            YInput.Editor = new FloatEditorViewModel();
            Inputs.Add(YInput);

            ZInput.Name = "Z";
            ZInput.Editor = new FloatEditorViewModel();
            Inputs.Add(ZInput);

            Result.Name = "Vec3";
            Result.ReturnType = typeof(Vec3);
            Result.Value = this.WhenAnyValue(vm => vm.XInput.Value, vm => vm.YInput.Value, vm => vm.ZInput.Value)
                .Select(t =>
                {
                    if (t.Item1 == null || t.Item2 == null || t.Item3 == null)
                    {
                        return null;
                    }
                    return new ShaderFunc(() =>
                        $"vec3(({t.Item1.Compile()}), ({t.Item2.Compile()}), ({t.Item3.Compile()}))");
                });
            Outputs.Add(Result);
        }
    }
}
