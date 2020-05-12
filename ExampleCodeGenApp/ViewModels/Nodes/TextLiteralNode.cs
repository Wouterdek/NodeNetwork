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
    public class TextLiteralNode : CodeGenNodeViewModel
    {
        static TextLiteralNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<TextLiteralNode>));
        }

        [DataMember] public StringValueEditorViewModel ValueEditor { get; set; } = new StringValueEditorViewModel();

        [DataMember] public ValueNodeOutputViewModel<ITypedExpression<string>> Output { get; }

        public TextLiteralNode() : base(NodeType.Literal)
        {
            this.Name = "Text";

            Output = new CodeGenOutputViewModel<ITypedExpression<string>>(PortType.String)
            {
                Name = "Value",
                Editor = ValueEditor,
                Value = ValueEditor.ValueChanged.Select(v => new StringLiteral{ Value = v })
            };
            this.Outputs.Add(Output);
        }
    }
}
