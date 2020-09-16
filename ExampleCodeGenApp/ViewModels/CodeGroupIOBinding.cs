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
    public class CodeGroupIOBinding : GroupIOBinding
    {
        private readonly IDictionary<NodeOutputViewModel, NodeInputViewModel> _outputInputMapping = new Dictionary<NodeOutputViewModel, NodeInputViewModel>();

        public ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<T>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Name = input.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this)
            };
        }

        public ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<IObservableList<T>>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Editor = new GroupEndpointEditorViewModel<IObservableList<T>>(this)
            };
        }

        public ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new CodeGenInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this)
            };
        }

        public ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new CodeGenListInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name,
                Editor = new GroupEndpointEditorViewModel<T>(this)
            };
        }

        private void BindOutputToInput<T>(ValueNodeOutputViewModel<T> output, ValueNodeInputViewModel<T> input)
        {
            output.Value = input.ValueChanged;
            _outputInputMapping.Add(output, input);
        }

        private void BindOutputToInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output, ValueListNodeInputViewModel<T> input)
        {
            output.Value = Observable.Return(input.Values);
            _outputInputMapping.Add(output, input);
        }

        public CodeGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode) 
            : base(groupNode, entranceNode, exitNode)
        {
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(i =>
                {
                    NodeOutputViewModel result = CreateCompatibleOutput((dynamic)i);
                    i.WhenAnyValue(vm => vm.Name).BindTo(result, vm => vm.Name);
                    i.WhenAnyValue(vm => vm.SortIndex).BindTo(result, vm => vm.SortIndex);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(entranceNode.Outputs);
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(i =>
                {
                    NodeOutputViewModel result = CreateCompatibleOutput((dynamic)i);
                    i.WhenAnyValue(vm => vm.Name).BindTo(result, vm => vm.Name);
                    i.WhenAnyValue(vm => vm.SortIndex).BindTo(result, vm => vm.SortIndex);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(exitNode.Outputs);
            groupNode.Inputs.Connect().OnItemRemoved(input => 
                    _outputInputMapping.Remove(
                    _outputInputMapping.First(kvp => kvp.Value == input)
                    )
                );
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput((dynamic)o);
                    o.WhenAnyValue(vm => vm.Name).BindTo(result, vm => vm.Name);
                    o.WhenAnyValue(vm => vm.SortIndex).BindTo(result, vm => vm.SortIndex);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(exitNode.Inputs);
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput((dynamic)o);
                    o.WhenAnyValue(vm => vm.Name).BindTo(result, vm => vm.Name);
                    o.WhenAnyValue(vm => vm.SortIndex).BindTo(result, vm => vm.SortIndex);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(entranceNode.Inputs);
            groupNode.Outputs.Connect().OnItemRemoved(output => _outputInputMapping.Remove(output));
        }

        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel input = CreateCompatibleInput((dynamic)candidateOutput);
            input.SortIndex = GroupNode.Inputs.Items.Select(i => i.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            GroupNode.Inputs.Add(input);
            return input;
        }

        public override NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput)
        {
            NodeInputViewModel input = AddNewGroupNodeInput(CreateCompatibleOutput((dynamic)candidateInput));
            return GetSubnetInlet(input);
        }

        public override NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput)
        {
            NodeOutputViewModel output = AddNewGroupNodeOutput(CreateCompatibleInput((dynamic)candidateOutput));
            return GetSubnetOutlet(output);
        }

        public override NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput)
        {
            NodeOutputViewModel output = CreateCompatibleOutput((dynamic)candidateInput);
            output.SortIndex = GroupNode.Outputs.Items.Select(o => o.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            GroupNode.Outputs.Add(output);
            return output;
        }


        public override NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel entranceOutput)
        {
            return _outputInputMapping[entranceOutput];
        }

        public override NodeOutputViewModel GetSubnetInlet(NodeInputViewModel entranceInput)
        {
            return _outputInputMapping.Single(p => p.Value == entranceInput).Key;
        }

        public override NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel exitOutput)
        {
            return _outputInputMapping[exitOutput];
        }

        public override NodeOutputViewModel GetGroupNodeOutput(NodeInputViewModel exitInput)
        {
            return _outputInputMapping.Single(p => p.Value == exitInput).Key;
        }
    }
}