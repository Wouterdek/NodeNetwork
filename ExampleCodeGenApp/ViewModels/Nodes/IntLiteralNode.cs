using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class IntLiteralNode : CodeGenNodeViewModel
    {
        static IntLiteralNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<IntLiteralNode>));
        }

        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();

        public ValueNodeOutputViewModel<ITypedExpression<int>> Output { get; }

        public IntLiteralNode() : base(NodeType.Literal)
        {
            this.Name = "Integer";

            Output = new CodeGenOutputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Editor = ValueEditor,
                Value = ValueEditor.ValueChanged.Select(v => new IntLiteral{Value = v ?? 0})
            };
            this.Outputs.Add(Output);
        }
    }
}
