using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.ViewModels.Editors;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class Vec3MathNodeViewModel : ShaderNodeViewModel
    {
        static Vec3MathNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec3MathNodeViewModel>));
        }

        enum MathOperation
        {
            Add, Subtract, Multiply, Divide, CrossProduct, DotProduct, Distance, Reflect, Min, Max
        }

        public ValueNodeInputViewModel<object> OperationInput { get; } = new ValueNodeInputViewModel<object>();
        public ShaderNodeInputViewModel InputA { get; } = new ShaderNodeInputViewModel(typeof(Vec3));
        public ShaderNodeInputViewModel InputB { get; } = new ShaderNodeInputViewModel(typeof(Vec3));

        public ShaderNodeOutputViewModel ResultVector { get; } = new ShaderNodeOutputViewModel();
        public ShaderNodeOutputViewModel ResultFloat { get; } = new ShaderNodeOutputViewModel();

        public Vec3MathNodeViewModel()
        {
            this.Name = "Vec3 Math";
            this.Category = NodeCategory.Math;

            OperationInput.Editor = new EnumEditorViewModel(typeof(MathOperation));
            OperationInput.Port.IsVisible = false;
            Inputs.Add(OperationInput);

            InputA.Name = "A";
            InputA.Editor = new Vec3EditorViewModel();
            Inputs.Add(InputA);

            InputB.Name = "B";
            InputB.Editor = new Vec3EditorViewModel();
            Inputs.Add(InputB);

            ResultVector.Name = "Result Vector";
            ResultVector.ReturnType = typeof(Vec3);
            ResultVector.Value = this.WhenAnyValue(vm => vm.InputA.Value, vm => vm.InputB.Value, vm => vm.OperationInput.Value)
                .Select(t =>
                {
                    if (t.Item1 == null || t.Item2 == null || t.Item3 == null)
                    {
                        return null;
                    }
                    return BuildMathVectorOperation(t.Item1, t.Item2, (MathOperation) t.Item3);
                });
            Outputs.Add(ResultVector);

            ResultFloat.Name = "Result Float";
            ResultFloat.ReturnType = typeof(float);
            ResultFloat.Value = this.WhenAnyValue(vm => vm.InputA.Value, vm => vm.InputB.Value, vm => vm.OperationInput.Value)
                .Select(t =>
                {
                    if (t.Item1 == null || t.Item2 == null || t.Item3 == null)
                    {
                        return null;
                    }
                    return BuildMathFloatOperation(t.Item1, t.Item2, (MathOperation)t.Item3);
                });
            Outputs.Add(ResultFloat);
        }

        private ShaderFunc BuildMathVectorOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
        {
            switch (operation)
            {
                case MathOperation.Add:
                    return new ShaderFunc(() => $"({a.Compile()}) + ({b.Compile()})");
                case MathOperation.Subtract:
                    return new ShaderFunc(() => $"({a.Compile()}) - ({b.Compile()})");
                case MathOperation.Multiply:
                    return new ShaderFunc(() => $"({a.Compile()}) * ({b.Compile()})");
                case MathOperation.Divide:
                    return new ShaderFunc(() => $"({a.Compile()}) / ({b.Compile()})");
                case MathOperation.CrossProduct:
                    return new ShaderFunc(() => $"cross(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Reflect:
                    return new ShaderFunc(() => $"reflect(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Min:
                    return new ShaderFunc(() => $"min(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Max:
                    return new ShaderFunc(() => $"max(({a.Compile()}), ({b.Compile()}))");
                default: return null;
            }
        }

        private ShaderFunc BuildMathFloatOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
        {
            switch (operation)
            {
                case MathOperation.DotProduct:
                    return new ShaderFunc(() => $"dot(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Distance:
                    return new ShaderFunc(() => $"distance(({a.Compile()}), ({b.Compile()}))");
                default: return null;
            }
        }
    }
}
