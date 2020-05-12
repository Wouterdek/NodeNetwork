using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.ViewModels.Editors;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public class Math2NodeViewModel : ShaderNodeViewModel
    {
        static Math2NodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Math2NodeViewModel>));
        }

        [DataContract]
        public enum MathOperation
        {
            [DataMember] Add,
            [DataMember] Subtract,
            [DataMember] Multiply,
            [DataMember] Divide,
            [DataMember] Power,
            [DataMember] Minimum,
            [DataMember] Maximum,
            [DataMember] LessThan,
            [DataMember] GreaterThan,
            [DataMember] Modulo
        }

        [DataMember] public ValueNodeInputViewModel<object> OperationInput { get; set; } = new ValueNodeInputViewModel<object>();
        [DataMember] public ShaderNodeInputViewModel InputA { get; set; } = new ShaderNodeInputViewModel(typeof(float));
        [DataMember] public ShaderNodeInputViewModel InputB { get; set; } = new ShaderNodeInputViewModel(typeof(float));

        [DataMember] public ShaderNodeOutputViewModel Result { get; set; } = new ShaderNodeOutputViewModel();

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
                    return BuildMathOperation(t.Item1, t.Item2, GetMathOperation(t.Item3));
                });
            Outputs.Add(Result);
        }

        private MathOperation GetMathOperation(object value)
        {
            MathOperation ret = default;
            try
            {
                ret = (MathOperation)value;
            }
            catch { }
            return ret;
        }

        private ShaderFunc BuildMathOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Add => new ShaderFunc(() => $"({a.Compile()}) + ({b.Compile()})"),
                MathOperation.Subtract => new ShaderFunc(() => $"({a.Compile()}) - ({b.Compile()})"),
                MathOperation.Multiply => new ShaderFunc(() => $"({a.Compile()}) * ({b.Compile()})"),
                MathOperation.Divide => new ShaderFunc(() => $"({a.Compile()}) / ({b.Compile()})"),
                MathOperation.Power => new ShaderFunc(() => $"pow(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Minimum => new ShaderFunc(() => $"min(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Maximum => new ShaderFunc(() => $"max(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.LessThan => new ShaderFunc(() => $"({a.Compile()}) < ({b.Compile()}) ? 1 : 0"),
                MathOperation.GreaterThan => new ShaderFunc(() => $"({a.Compile()}) > ({b.Compile()}) ? 1 : 0"),
                MathOperation.Modulo => new ShaderFunc(() => $"mod(({a.Compile()}), ({b.Compile()}))"),
                _ => throw new Exception("Unsupported math operation"),
            };
        }
    }
}
