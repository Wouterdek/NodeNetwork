using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Group
{
    public abstract class GroupIOBinding
    {
        /// <summary>
        /// Node in the parent network that represents the group.
        /// </summary>
        public NodeViewModel GroupNode { get; }

        /// <summary>
        /// Input node in the subnet
        /// </summary>
        public NodeViewModel EntranceNode { get; }

        /// <summary>
        /// Output node in the subnet
        /// </summary>
        public NodeViewModel ExitNode { get; }

        public GroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode)
        {
            GroupNode = groupNode;
            EntranceNode = entranceNode;
            ExitNode = exitNode;
        }

        public abstract NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel subnetInlet);

        public abstract NodeOutputViewModel GetSubnetInlet(NodeInputViewModel entranceInput);

        public abstract NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel groupNodeOutput);

        public abstract NodeOutputViewModel GetGroupNodeOutput(NodeInputViewModel subnetOutlet);

        /// <summary>
        /// Create and add a new input to the group node, along with a corresponding output in the subnet (e.g. on the entrance node).
        /// </summary>
        /// <param name="candidateOutput">Output viewmodel that should match the new input on the group node.</param>
        /// <returns></returns>
        public abstract NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput);

        /// <summary>
        /// Create and add a new input to the group node, along with a corresponding output in the subnet (e.g. on the entrance node).
        /// </summary>
        /// <param name="candidateInput">Input viewmodel that should match the new output that is added to the subnet.</param>
        /// <returns></returns>
        public abstract NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput);

        /// <summary>
        /// Create and add a new output to the group node, along with a corresponding input in the subnet (e.g. on the exit node).
        /// </summary>
        /// <param name="candidateInput">Input viewmodel that should match the new output on the group node.</param>
        /// <returns></returns>
        public abstract NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput);

        /// <summary>
        /// Create and add a new output to the group node, along with a corresponding input in the subnet (e.g. on the exit node).
        /// </summary>
        /// <param name="candidateOutput">Output viewmodel that should match the new input that is added to the subnet.</param>
        /// <returns></returns>
        public abstract NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput);
    }
}