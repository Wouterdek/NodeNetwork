using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    [DataContract]
    public class IntLiteralNode : CodeGenNodeViewModel
    {
        static IntLiteralNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<IntLiteralNode>));
        }

        [DataMember] public IntegerValueEditorViewModel ValueEditor { get; set; } = new IntegerValueEditorViewModel();

        [DataMember] public ValueNodeOutputViewModel<ITypedExpression<int>> Output { get; set; }

        public IntLiteralNode() : base(NodeType.Literal)
        {
            this.Name = "Integer";

            Output = new CodeGenOutputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Editor = ValueEditor,
                Value = ValueEditor.ValueChanged.Select(v => new IntLiteral { Value = v ?? 0 })
            };
            this.Outputs.Add(Output);
        }
    }
}
