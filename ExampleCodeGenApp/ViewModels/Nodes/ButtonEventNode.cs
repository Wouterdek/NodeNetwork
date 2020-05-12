using System.Runtime.Serialization;
using DynamicData;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    [DataContract]
    public class ButtonEventNode : CodeGenNodeViewModel, IAmTheOutputViewModel
    {
        static ButtonEventNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<ButtonEventNode>));
        }

        [DataMember] public ValueListNodeInputViewModel<IStatement> OnClickFlow { get; set; }

        public ButtonEventNode() : base(NodeType.EventNode)
        {
            this.Name = "Button Events";

            OnClickFlow = new CodeGenListInputViewModel<IStatement>(PortType.Execution)
            {
                Name = "On Click"
            };

            this.Inputs.Add(OnClickFlow);
        }
    }
}
