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
    public class Math2NodeViewModel : ShaderNodeViewModel
    {
        static Math2NodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Math2NodeViewModel>));
        }

        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Power,
            Minimum,
            Maximum,
            LessThan,
            GreaterThan,
            Modulo
        }

        public ValueNodeInputViewModel<object> OperationInput { get; } = new ValueNodeInputViewModel<object>();
        public ShaderNodeInputViewModel InputA { get; } = new ShaderNodeInputViewModel(typeof(float));
        public ShaderNodeInputViewModel InputB { get; } = new ShaderNodeInputViewModel(typeof(float));

        public ShaderNodeOutputViewModel Result { get; } = new ShaderNodeOutputViewModel();

        public Math2NodeViewModel()
        {
            this.Name = "Math 2";
            this.Category = NodeCategory.Math;

            OperationInput.Editor = new EnumEditorViewModel(typeof(MathOperation));
            OperationInput.Port.IsVisible = false;
            Inputs.Add(OperationInput);

            InputA.Name = "A";
            InputA.Editor = new FloatEditorViewModel();
            Inputs.Add(InputA);

            InputB.Name = "B";
            InputB.Editor = new FloatEditorViewModel();
            Inputs.Add(InputB);

            Result.Name = "Result";
            Result.ReturnType = typeof(float);
            Result.Value = this.WhenAnyValue(vm => vm.InputA.Value, vm => vm.InputB.Value, vm => vm.OperationInput.Value)
                .Select(t =>
                {
                    if (t.Item1 == null || t.Item2 == null)
                    {
                        return null;
                    }
                    return BuildMathOperation(t.Item1, t.Item2, (MathOperation) t.Item3);
                });
            Outputs.Add(Result);
        }

        private ShaderFunc BuildMathOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
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
                case MathOperation.Power:
                    return new ShaderFunc(() => $"pow(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Minimum:
                    return new ShaderFunc(() => $"min(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.Maximum:
                    return new ShaderFunc(() => $"max(({a.Compile()}), ({b.Compile()}))");
                case MathOperation.LessThan:
                    return new ShaderFunc(() => $"({a.Compile()}) < ({b.Compile()}) ? 1 : 0");
                case MathOperation.GreaterThan:
                    return new ShaderFunc(() => $"({a.Compile()}) > ({b.Compile()}) ? 1 : 0");
                case MathOperation.Modulo:
                    return new ShaderFunc(() => $"mod(({a.Compile()}), ({b.Compile()}))");
                default:
                    throw new Exception("Unsupported math operation");
            }
        }
    }
}
