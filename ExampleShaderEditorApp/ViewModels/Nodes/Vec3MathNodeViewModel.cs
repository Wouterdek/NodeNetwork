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
    public class Vec3MathNodeViewModel : ShaderNodeViewModel
    {
        static Vec3MathNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Vec3MathNodeViewModel>));
        }

        [DataContract]
        enum MathOperation
        {
            [DataMember] Add, [DataMember] Subtract, [DataMember] Multiply, [DataMember] Divide, [DataMember] CrossProduct, [DataMember] DotProduct, [DataMember] Distance, [DataMember] Reflect, [DataMember] Min, [DataMember] Max
        }

        [DataMember] public ValueNodeInputViewModel<object> OperationInput { get; set; } = new ValueNodeInputViewModel<object>();
        [DataMember] public ShaderNodeInputViewModel InputA { get; set; } = new ShaderNodeInputViewModel(typeof(Vec3));
        [DataMember] public ShaderNodeInputViewModel InputB { get; set; } = new ShaderNodeInputViewModel(typeof(Vec3));

        [DataMember] public ShaderNodeOutputViewModel ResultVector { get; set; } = new ShaderNodeOutputViewModel();
        [DataMember] public ShaderNodeOutputViewModel ResultFloat { get; set; } = new ShaderNodeOutputViewModel();

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
                    return BuildMathVectorOperation(t.Item1, t.Item2, GetMathOperation(t.Item3));
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
                    return BuildMathFloatOperation(t.Item1, t.Item2, GetMathOperation(t.Item3));
                });
            Outputs.Add(ResultFloat);
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

        private ShaderFunc BuildMathVectorOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Add => new ShaderFunc(() => $"({a.Compile()}) + ({b.Compile()})"),
                MathOperation.Subtract => new ShaderFunc(() => $"({a.Compile()}) - ({b.Compile()})"),
                MathOperation.Multiply => new ShaderFunc(() => $"({a.Compile()}) * ({b.Compile()})"),
                MathOperation.Divide => new ShaderFunc(() => $"({a.Compile()}) / ({b.Compile()})"),
                MathOperation.CrossProduct => new ShaderFunc(() => $"cross(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Reflect => new ShaderFunc(() => $"reflect(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Min => new ShaderFunc(() => $"min(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Max => new ShaderFunc(() => $"max(({a.Compile()}), ({b.Compile()}))"),
                _ => null,
            };
        }

        private ShaderFunc BuildMathFloatOperation(ShaderFunc a, ShaderFunc b, MathOperation operation)
        {
            return operation switch
            {
                MathOperation.DotProduct => new ShaderFunc(() => $"dot(({a.Compile()}), ({b.Compile()}))"),
                MathOperation.Distance => new ShaderFunc(() => $"distance(({a.Compile()}), ({b.Compile()}))"),
                _ => null,
            };
        }
    }
}
