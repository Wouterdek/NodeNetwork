using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// Facilitates connections between nodes outside and inside a group.
    /// This is performed by having inputs on the group node (in the supernet) that map to outputs on (mostly) the EntranceNode in the subnet.
    /// Likewise, outputs of the group node map to inputs on (mostly) the ExitNode in the subnet.
    /// </summary>
    public abstract class NodeGroupIOBinding
    {
        /// <summary>
        /// Node in the parent network that represents the group.
        /// </summary>
        public NodeViewModel GroupNode { get; }

        /// <summary>
        /// Inlet node in the subnet.
        /// Although this generally contains only outputs, this may contain inputs if their orientation is flipped.
        /// </summary>
        public NodeViewModel EntranceNode { get; }

        /// <summary>
        /// Outlet node in the subnet.
        /// Although this generally contains only outputs, this may contain inputs if their orientation is flipped.
        /// </summary>
        public NodeViewModel ExitNode { get; }

        /// <summary>
        /// Parent network that contains the GroupNode.
        /// </summary>
        public NetworkViewModel SuperNetwork => GroupNode.Parent;

        /// <summary>
        /// Child network, contained in SuperNetwork, that contains the group member nodes (like the EntranceNode and ExitNode).
        /// </summary>
        public NetworkViewModel SubNetwork => ExitNode.Parent;

        public NodeGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode)
        {
            GroupNode = groupNode;
            EntranceNode = entranceNode;
            ExitNode = exitNode;
        }

        /// <summary>
        /// Given the output in the subnet, return the corresponding input on the groupnode in the supernet.
        /// </summary>
        public abstract NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel subnetInlet);

        /// <summary>
        /// Given the input on the group node in the supernet, return the corresponding output in the subnet.
        /// </summary>
        public abstract NodeOutputViewModel GetSubnetInlet(NodeInputViewModel entranceInput);

        /// <summary>
        /// Given the output on the group node in the supernet, return the corresponding input in the subnet.
        /// </summary>
        public abstract NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel groupNodeOutput);

        /// <summary>
        /// Given the input in the subnet, return the corresponding output on the groupnode in the supernet.
        /// </summary>
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
        public abstract NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput);

        /// <summary>
        /// Create and add a new output to the group node, along with a corresponding input in the subnet (e.g. on the exit node).
        /// </summary>
        /// <param name="candidateInput">Input viewmodel that should match the new output on the group node.</param>
        public abstract NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput);

        /// <summary>
        /// Create and add a new output to the group node, along with a corresponding input in the subnet (e.g. on the exit node).
        /// </summary>
        /// <param name="candidateOutput">Output viewmodel that should match the new input that is added to the subnet.</param>
        public abstract NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput);
    }
}