using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodeNodeGroupIOBinding : ValueNodeGroupIOBinding
    {
        public CodeNodeGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode) : base(groupNode, entranceNode, exitNode)
        {
        }

        #region Endpoint Create
        public override ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<T>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Name = input.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this)
            };
        }

        public override ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<IObservableList<T>>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Editor = new GroupEndpointEditorViewModel<IObservableList<T>>(this)
            };
        }

        public override ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new CodeGenInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this),
                HideEditorIfConnected = false
            };
        }

        public override ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new CodeGenListInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this),
                HideEditorIfConnected = false
            };
        }
        #endregion
    }
}