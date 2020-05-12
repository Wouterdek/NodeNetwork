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
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public class MathNodeViewModel : ShaderNodeViewModel
    {
        static MathNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<MathNodeViewModel>));
        }

        [DataContract]
        public enum MathOperation
        {
            [DataMember] Sine,
            [DataMember] Cosine,
            [DataMember] Tangent,
            [DataMember] Arcsine,
            [DataMember] Arccosine,
            [DataMember] Arctangent,
            [DataMember] Logarithm,
            [DataMember] Round,
            [DataMember] Floor,
            [DataMember] Ceil,
            [DataMember] Absolute,
            [DataMember] Sign,
            [DataMember] Sqrt
        }

        [DataMember] public ValueNodeInputViewModel<object> OperationInput { get; } = new ValueNodeInputViewModel<object>();
        [DataMember] public ShaderNodeInputViewModel Input { get; set; } = new ShaderNodeInputViewModel(typeof(float));

        [DataMember] public ShaderNodeOutputViewModel Result { get; set; } = new ShaderNodeOutputViewModel();

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
                .Select(t => (t.Item1 == null || t.Item2 == null) ? null : BuildMathOperation(t.Item1, GetMathOperation(t.Item2)));
            Outputs.Add(Result);
        }

        private MathOperation GetMathOperation(object value)
        {
            // to reslove issue int64 to MathOperation @ item2
            MathOperation ret = default;
            try
            {
                ret = (MathOperation)value;
            }
            catch { }
            return ret;
        }

        private ShaderFunc BuildMathOperation(ShaderFunc a, MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Sine => new ShaderFunc(() => $"sin({a.Compile()})"),
                MathOperation.Cosine => new ShaderFunc(() => $"cos({a.Compile()})"),
                MathOperation.Tangent => new ShaderFunc(() => $"tan({a.Compile()})"),
                MathOperation.Arcsine => new ShaderFunc(() => $"asin({a.Compile()})"),
                MathOperation.Arccosine => new ShaderFunc(() => $"acos({a.Compile()})"),
                MathOperation.Arctangent => new ShaderFunc(() => $"atan({a.Compile()})"),
                MathOperation.Logarithm => new ShaderFunc(() => $"log({a.Compile()})"),
                MathOperation.Round => new ShaderFunc(() => $"round({a.Compile()})"),
                MathOperation.Floor => new ShaderFunc(() => $"floor({a.Compile()})"),
                MathOperation.Ceil => new ShaderFunc(() => $"ceil({a.Compile()})"),
                MathOperation.Absolute => new ShaderFunc(() => $"abs({a.Compile()})"),
                MathOperation.Sign => new ShaderFunc(() => $"sign({a.Compile()})"),
                MathOperation.Sqrt => new ShaderFunc(() => $"sqrt({a.Compile()})"),
                _ => throw new Exception("Unsupported math operation"),
            };
        }
    }
}
