using System.Runtime.Serialization;
using ExampleCodeGenApp.Views;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    [DataContract]
    public enum NodeType
    {
        [DataMember] EventNode, [DataMember] Function, [DataMember] FlowControl, [DataMember] Literal
    }

    [DataContract]
    public class CodeGenNodeViewModel : NodeViewModel
    {
        static CodeGenNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<CodeGenNodeViewModel>));
        }

        [DataMember] public NodeType NodeType { get; set; }

        public CodeGenNodeViewModel(NodeType type)
        {
            NodeType = type;
        }
    }
}
