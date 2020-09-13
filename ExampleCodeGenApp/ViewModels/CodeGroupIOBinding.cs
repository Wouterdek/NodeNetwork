using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodeGroupIOBinding : GroupIOBinding
    {
        private readonly IDictionary<NodeOutputViewModel, NodeInputViewModel> _outputInputMapping = new Dictionary<NodeOutputViewModel, NodeInputViewModel>();

        public ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<T>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Name = input.Name
            };
        }

        public ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<IObservableList<T>>(((CodeGenPortViewModel)input.Port).PortType);
        }

        public ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new CodeGenInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name
            };
        }

        public ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new CodeGenListInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name
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
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(entranceNode.Outputs);
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(i =>
                {
                    NodeOutputViewModel result = CreateCompatibleOutput((dynamic)i);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(exitNode.Outputs);
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput((dynamic)o);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(exitNode.Inputs);
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput((dynamic)o);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(entranceNode.Inputs);
        }

        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel input = CreateCompatibleInput((dynamic)candidateOutput);
            GroupNode.Inputs.Add(input);
            return input;
        }

        public override NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput)
        {
            NodeInputViewModel input = AddNewGroupNodeInput(CreateCompatibleOutput((dynamic)candidateInput));
            int idx = GroupNode.Inputs.Items.IndexOf(input);
            return EntranceNode.Outputs.Items.ElementAt(idx);
        }

        public override NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput)
        {
            NodeOutputViewModel output = AddNewGroupNodeOutput(CreateCompatibleInput((dynamic)candidateOutput));
            int idx = GroupNode.Outputs.Items.IndexOf(output);
            return ExitNode.Inputs.Items.ElementAt(idx);
        }

        public override NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput)
        {
            NodeOutputViewModel output = CreateCompatibleOutput((dynamic)candidateInput);
            GroupNode.Outputs.Add(output);
            return output;
        }


        public override NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel entranceOutput)
        {
            return _outputInputMapping[entranceOutput];
            //return GroupNode.Inputs.Items.ElementAt(EntranceNode.Outputs.Items.IndexOf(entranceOutput));
        }

        public override NodeOutputViewModel GetSubnetInlet(NodeInputViewModel entranceInput)
        {
            return _outputInputMapping.Single(p => p.Value == entranceInput).Key;
            //var node = entranceInput.PortPosition == PortPosition.Left ? EntranceNode : ExitNode;
            //return node.Outputs.Items.ElementAt(GroupNode.Inputs.Items.IndexOf(entranceInput));
        }

        public override NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel exitOutput)
        {
            return _outputInputMapping[exitOutput];
            //var node = exitOutput.PortPosition == PortPosition.Right ? ExitNode : EntranceNode;
            //return ExitNode.Inputs.Items.ElementAt(GroupNode.Outputs.Items.IndexOf(exitOutput));
        }

        public override NodeOutputViewModel GetGroupNodeOutput(NodeInputViewModel exitInput)
        {
            return _outputInputMapping.Single(p => p.Value == exitInput).Key;
            //return GroupNode.Outputs.Items.ElementAt(ExitNode.Inputs.Items.IndexOf(exitInput));
        }
    }
}