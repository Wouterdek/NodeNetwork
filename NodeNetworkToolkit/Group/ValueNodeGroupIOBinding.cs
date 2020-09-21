using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// Basic reference implementation of NodeGroupIOBinding for ValueInputViewModels and ValueOutputViewModels.
    /// </summary>
    public class ValueNodeGroupIOBinding : NodeGroupIOBinding
    {
        private readonly IDictionary<NodeOutputViewModel, NodeInputViewModel> _outputInputMapping = new Dictionary<NodeOutputViewModel, NodeInputViewModel>();

        public ValueNodeGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode) 
            : base(groupNode, entranceNode, exitNode)
        {
            // For each input on the group node, create an output in the subnet
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(i =>
                {
                    // Dynamic is applied here so that late binding is used to find the most specific 
                    // CreateCompatibleOutput variant for this specific input.
                    NodeOutputViewModel result = CreateCompatibleOutput((dynamic)i);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(entranceNode.Outputs);
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(i =>
                {
                    NodeOutputViewModel result = CreateCompatibleOutput((dynamic) i);
                    BindOutputToInput((dynamic) result, (dynamic) i);
                    return result;
                }).PopulateInto(exitNode.Outputs);
            groupNode.Inputs.Connect().OnItemRemoved(input => 
                    _outputInputMapping.Remove(
                    _outputInputMapping.First(kvp => kvp.Value == input)
                    )
                );

            // For each output on the group node, create an input in the subnet
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
            groupNode.Outputs.Connect().OnItemRemoved(output => _outputInputMapping.Remove(output));
        }

        protected virtual void BindEndpointProperties(NodeOutputViewModel output, NodeInputViewModel input)
        {
            input.WhenAnyValue(vm => vm.Name).BindTo(output, vm => vm.Name);
            output.WhenAnyValue(vm => vm.Name).BindTo(input, vm => vm.Name);
            input.WhenAnyValue(vm => vm.SortIndex).BindTo(output, vm => vm.SortIndex);
            output.WhenAnyValue(vm => vm.SortIndex).BindTo(input, vm => vm.SortIndex);
            input.WhenAnyValue(vm => vm.Icon).BindTo(output, vm => vm.Icon);
            output.WhenAnyValue(vm => vm.Icon).BindTo(input, vm => vm.Icon);
        }

        protected virtual void BindOutputToInput<T>(ValueNodeOutputViewModel<T> output, ValueNodeInputViewModel<T> input)
        {
            BindEndpointProperties(output, input);
            output.Value = input.ValueChanged;
            _outputInputMapping.Add(output, input);
        }

        protected virtual void BindOutputToInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output, ValueListNodeInputViewModel<T> input)
        {
            BindEndpointProperties(output, input);
            output.Value = Observable.Return(input.Values);
            _outputInputMapping.Add(output, input);
        }

        #region Endpoint Create
        public virtual ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new ValueNodeOutputViewModel<T>()
            {
                Name = input.Name,
                Icon = input.Icon
            };
        }

        public virtual ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new ValueNodeOutputViewModel<IObservableList<T>>();
        }

        public virtual ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new ValueNodeInputViewModel<T>()
            {
                Name = output.Name,
                Icon = output.Icon
            };
        }

        public virtual ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new ValueListNodeInputViewModel<T>()
            {
                Name = output.Name,
                Icon = output.Icon
            };
        }
        #endregion

        #region Endpoint Add
        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel input = CreateCompatibleInput((dynamic)candidateOutput);
            GroupNode.Inputs.Add(input);
            // Append to bottom of list
            input.SortIndex = GroupNode.Inputs.Items.Select(i => i.SortIndex).DefaultIfEmpty(-1).Max() + 1;
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
            GroupNode.Outputs.Add(output);
            // Append to bottom of list
            output.SortIndex = GroupNode.Outputs.Items.Select(o => o.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            return output;
        }
        #endregion

        #region Endpoint Getters
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
        #endregion

        /// <summary>
        /// Remove an endpoint, which can be from group node, entrance node or exit node.
        /// Also removes the corresponding endpoint in the other network.
        /// </summary>
        /// <param name="endpoint">Input or output to be removed.</param>
        public virtual void DeleteEndpoint(Endpoint endpoint)
        {
            // Because the subnet entrance and exit are derived from the groupnode,
            // endpoints should be deleted from the groupnode only.

            bool isOnGroupNode = GroupNode == endpoint.Parent;

            if (endpoint is NodeInputViewModel input)
            {
                if (isOnGroupNode)
                {
                    GroupNode.Inputs.Remove(input);
                }
                else
                {
                    var groupOutput = GetGroupNodeOutput(input);
                    GroupNode.Outputs.Remove(groupOutput);
                }
            }
            else if(endpoint is NodeOutputViewModel output)
            {
                if (isOnGroupNode)
                {
                    GroupNode.Outputs.Remove(output);
                }
                else
                {
                    var groupInput = GetGroupNodeInput(output);
                    GroupNode.Inputs.Remove(groupInput);
                }
            }
        }
    }
}