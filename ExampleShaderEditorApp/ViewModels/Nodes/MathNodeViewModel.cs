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
    public class MathNodeViewModel : ShaderNodeViewModel
    {
        static MathNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<MathNodeViewModel>));
        }

        public enum MathOperation
        {
            Sine,
            Cosine,
            Tangent,
            Arcsine,
            Arccosine,
            Arctangent,
            Logarithm,
            Round,
            Floor,
            Ceil,
            Absolute,
            Sign,
            Sqrt
        }

        public ValueNodeInputViewModel<object> OperationInput { get; } = new ValueNodeInputViewModel<object>();
        public ShaderNodeInputViewModel Input { get; } = new ShaderNodeInputViewModel(typeof(float));

        public ShaderNodeOutputViewModel Result { get; } = new ShaderNodeOutputViewModel();

        public MathNodeViewModel()
        {
            this.Name = "Math";
            this.Category = NodeCategory.Math;

            OperationInput.Editor = new EnumEditorViewModel(typeof(MathOperation));
            OperationInput.Port.IsVisible = false;
            Inputs.Add(OperationInput);

            Input.Name = "A";
            Input.Editor = new FloatEditorViewModel();
            Inputs.Add(Input);
            
            Result.Name = "Result";
            Result.ReturnType = typeof(float);
            Result.Value = this.WhenAnyValue(vm => vm.Input.Value, vm => vm.OperationInput.Value)
                .Select(t => (t.Item1 == null || t.Item2 == null) ? null : BuildMathOperation(t.Item1, (MathOperation)t.Item2));
            Outputs.Add(Result);
        }

        private ShaderFunc BuildMathOperation(ShaderFunc a, MathOperation operation)
        {
            switch (operation)
            {
                case MathOperation.Sine:
                    return new ShaderFunc(() => $"sin({a.Compile()})");
                case MathOperation.Cosine:
                    return new ShaderFunc(() => $"cos({a.Compile()})");
                case MathOperation.Tangent:
                    return new ShaderFunc(() => $"tan({a.Compile()})");
                case MathOperation.Arcsine:
                    return new ShaderFunc(() => $"asin({a.Compile()})");
                case MathOperation.Arccosine:
                    return new ShaderFunc(() => $"acos({a.Compile()})");
                case MathOperation.Arctangent:
                    return new ShaderFunc(() => $"atan({a.Compile()})");
                case MathOperation.Logarithm:
                    return new ShaderFunc(() => $"log({a.Compile()})");
                case MathOperation.Round:
                    return new ShaderFunc(() => $"round({a.Compile()})");
                case MathOperation.Floor:
                    return new ShaderFunc(() => $"floor({a.Compile()})");
                case MathOperation.Ceil:
                    return new ShaderFunc(() => $"ceil({a.Compile()})");
                case MathOperation.Absolute:
                    return new ShaderFunc(() => $"abs({a.Compile()})");
                case MathOperation.Sign:
                    return new ShaderFunc(() => $"sign({a.Compile()})");
                case MathOperation.Sqrt:
                    return new ShaderFunc(() => $"sqrt({a.Compile()})");
                default:
                    throw new Exception("Unsupported math operation");
            }
        }
    }
}
