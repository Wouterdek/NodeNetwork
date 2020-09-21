using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using DynamicData.Alias;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// Used to provide nesting of networks by grouping nodes.
    /// </summary>
    public class NodeGrouper
    {
        /// <summary>
        /// Constructs a new node that represents a group of nodes.
        /// The parameter is the subnetwork (constructed with SubNetworkFactory) that contains the group member nodes.
        /// </summary>
        public Func<NetworkViewModel, NodeViewModel> GroupNodeFactory { get; set; } = subnet => new NodeViewModel();

        /// <summary>
        /// Constructs a viewmodel for the subnetwork that will contain the group member nodes.
        /// </summary>
        public Func<NetworkViewModel> SubNetworkFactory { get; set; } = () => new NetworkViewModel();

        /// <summary>
        /// Constructs the node in the subnet that provides access to (mostly) inputs to the group
        /// </summary>
        public Func<NodeViewModel> EntranceNodeFactory { get; set; } = () => new NodeViewModel();

        /// <summary>
        /// Constructs the node in the subnet that provides access to (mostly) outputs of the group
        /// </summary>
        public Func<NodeViewModel> ExitNodeFactory { get; set; } = () => new NodeViewModel();

        /// <summary>
        /// Constructs a NodeGroupIOBinding from a group, entrance and exit node.
        /// </summary>
        public Func<NodeViewModel, NodeViewModel, NodeViewModel, NodeGroupIOBinding> IOBindingFactory { get; set; }

        private bool CheckPropertiesValid() =>
            GroupNodeFactory != null && SubNetworkFactory != null && EntranceNodeFactory != null && ExitNodeFactory != null && IOBindingFactory != null;

        /// <summary>
        /// Move the specified set of nodes to a new subnetwork, create a new group node that contains this subnet,
        /// restore inter- and intra-network connections.
        /// </summary>
        /// <param name="network">The parent network</param>
        /// <param name="nodesToGroup">The nodes to group</param>
        /// <returns>Returns the NodeGroupIOBinding that was constructed for this group using the IOBindingFactory.</returns>
        public NodeGroupIOBinding MergeIntoGroup(NetworkViewModel network, IEnumerable<NodeViewModel> nodesToGroup)
        {
            if (!CheckPropertiesValid())
            {
                throw new InvalidOperationException("All properties must be set before usage");
            }
            else if (network == null)
            {
                throw new ArgumentNullException(nameof(network));
            }
            else if (nodesToGroup == null)
            {
                throw new ArgumentNullException(nameof(nodesToGroup));
            }

            var groupNodesSet = nodesToGroup is HashSet<NodeViewModel> set
                ? set
                : new HashSet<NodeViewModel>(nodesToGroup);

            // Check if nodesToGroup can be combined into a single group
            if (groupNodesSet.Count == 0 || !GraphAlgorithms.IsContinuousSubGraphSet(groupNodesSet))
            {
                return null;
            }

            // Create new empty group
            var subnet = SubNetworkFactory();

            var groupNode = GroupNodeFactory(subnet);
            network.Nodes.Add(groupNode);

            var groupEntranceNode = EntranceNodeFactory();
            var groupExitNode = ExitNodeFactory();

            subnet.Nodes.AddRange(new []{groupEntranceNode, groupExitNode});

            // Map from input on a group member node to group node input
            var groupNodeInputs = new Dictionary<NodeInputViewModel, NodeInputViewModel>();
            // Map from output on a group member node to group node output
            var groupNodeOutputs = new Dictionary<NodeOutputViewModel, NodeOutputViewModel>();

            // Move the new nodes to appropriate positions
            groupNode.Position = new Point(
                groupNodesSet.Average(n => n.Position.X),
                groupNodesSet.Average(n => n.Position.Y)
            );

            double yCoord = groupNodesSet.Average(n => n.Position.Y);
            groupEntranceNode.Position = new Point(
                groupNodesSet.Min(n => n.Position.X) - 100,
                yCoord
            );
            groupExitNode.Position = new Point(
                groupNodesSet.Max(n => n.Position.X) + 100,
                yCoord
            );

            // Setup binding between entrance/exit inputs and outputs
            var ioBinding = IOBindingFactory(groupNode, groupEntranceNode, groupExitNode);

            // Calculate set of connections to replace
            var subnetConnections = new List<ConnectionViewModel>();
            var borderInputConnections = new List<ConnectionViewModel>();
            var borderOutputConnections = new List<ConnectionViewModel>();
            foreach (var con in network.Connections.Items)
            {
                bool inputIsInSubnet = groupNodesSet.Contains(con.Input.Parent);
                bool outputIsInSubnet = groupNodesSet.Contains(con.Output.Parent);

                if (inputIsInSubnet && outputIsInSubnet)
                {
                    subnetConnections.Add(con);
                }
                else if (inputIsInSubnet)
                {
                    borderInputConnections.Add(con);
                }
                else if (outputIsInSubnet)
                {
                    borderOutputConnections.Add(con);
                }
            }

            // Construct inputs/outputs into/out of the group
            foreach (var borderInCon in borderInputConnections)
            {
                if (!groupNodeInputs.ContainsKey(borderInCon.Input))
                {
                    groupNodeInputs[borderInCon.Input] = ioBinding.AddNewGroupNodeInput(borderInCon.Output);
                }
            }

            foreach (var borderOutCon in borderOutputConnections)
            {
                if (!groupNodeOutputs.ContainsKey(borderOutCon.Output))
                {
                    groupNodeOutputs[borderOutCon.Output] = ioBinding.AddNewGroupNodeOutput(borderOutCon.Input);
                }
            }

            // Transfer nodes and inner connections to subnet
            network.Connections.Edit(l =>
            {
                l.RemoveMany(subnetConnections);
                l.RemoveMany(borderInputConnections);
                l.RemoveMany(borderOutputConnections);
            });
            network.Nodes.RemoveMany(groupNodesSet);
            subnet.Nodes.AddRange(groupNodesSet);
            subnet.Connections.AddRange(subnetConnections.Select(con => subnet.ConnectionFactory(con.Input, con.Output)));

            // Restore connections in/out of group
            network.Connections.AddRange(Enumerable.Concat(
                borderInputConnections.Select(con => network.ConnectionFactory(groupNodeInputs[con.Input], con.Output)),
                borderOutputConnections.Select(con => network.ConnectionFactory(con.Input, groupNodeOutputs[con.Output]))
            ));
            subnet.Connections.AddRange(Enumerable.Concat(
                borderInputConnections.Select(con => subnet.ConnectionFactory(con.Input, ioBinding.GetSubnetInlet(groupNodeInputs[con.Input]))),
                borderOutputConnections.Select(con => subnet.ConnectionFactory(ioBinding.GetSubnetOutlet(groupNodeOutputs[con.Output]), con.Output))
            ));

            return ioBinding;
        }

        /// <summary>
        /// Reverses the grouping performed by MergeIntoGroup.
        /// Group members get moved back into the parent network and the group node is removed.
        /// </summary>
        /// <param name="nodeGroupInfo">The NodeGroupIOBinding of the group to dissolve.</param>
        public void Ungroup(NodeGroupIOBinding nodeGroupInfo)
        {
            if (!CheckPropertiesValid())
            {
                throw new InvalidOperationException("All properties must be set before usage");
            }
            else if (nodeGroupInfo == null)
            {
                throw new ArgumentNullException(nameof(nodeGroupInfo));
            }

            var supernet = nodeGroupInfo.SuperNetwork;
            var subnet = nodeGroupInfo.SubNetwork;

            // Calculate set of subnet connections to replace
            var borderInputConnections = new List<Tuple<NodeOutputViewModel, NodeInputViewModel[]>>();
            var borderOutputConnections = new List<Tuple<NodeInputViewModel, NodeOutputViewModel[]>>();
            var subnetConnections = new List<ConnectionViewModel>();
            foreach (var conn in subnet.Connections.Items)
            {
                if (conn.Input.Parent == nodeGroupInfo.EntranceNode || conn.Input.Parent == nodeGroupInfo.ExitNode)
                {
                    var inputs = nodeGroupInfo.GetGroupNodeOutput(conn.Input).Connections.Items.Select(c => c.Input).ToArray();
                    if (inputs.Length > 0)
                    {
                        borderInputConnections.Add(Tuple.Create(conn.Output, inputs));
                    }
                }
                else if (conn.Output.Parent == nodeGroupInfo.EntranceNode || conn.Output.Parent == nodeGroupInfo.ExitNode)
                {
                    var outputs = nodeGroupInfo.GetGroupNodeInput(conn.Output).Connections.Items.Select(c => c.Output).ToArray();
                    if (outputs.Length > 0)
                    {
                        borderOutputConnections.Add(Tuple.Create(conn.Input, outputs));
                    }
                }
                else
                {
                    subnetConnections.Add(conn);
                }
            }

            // Calculate set of nodes to move
            var groupMemberNodes = subnet.Nodes.Items.Where(node => node != nodeGroupInfo.EntranceNode && node != nodeGroupInfo.ExitNode).ToArray();

            // Calculate center of nodes
            var minX = groupMemberNodes.Min(n => n.Position.X);
            var minY = groupMemberNodes.Min(n => n.Position.Y);
            var maxX = groupMemberNodes.Max(n => n.Position.X);
            var maxY = groupMemberNodes.Max(n => n.Position.Y);
            var center = new Vector(minX + (maxX - minX)/2, minY + (maxY - minY)/2);

            // Remove connections and nodes from subnet
            subnet.Connections.Clear();
            subnet.Nodes.Clear();

            // Remove groupnode and connections from supernet
            var groupNodePos = new Vector(nodeGroupInfo.GroupNode.Position.X, nodeGroupInfo.GroupNode.Position.Y);
            supernet.Nodes.Remove(nodeGroupInfo.GroupNode);

            // Add nodes to supernet and move them to correct position
            supernet.Nodes.AddRange(groupMemberNodes);
            foreach (var node in groupMemberNodes)
            {
                node.Position = node.Position - center + groupNodePos;
            }

            // Add connections to supernet
            supernet.Connections.AddRange(subnetConnections);
            foreach (var connTuple in borderInputConnections)
            {
                var output = connTuple.Item1;
                var inputs = connTuple.Item2;
                var connections = inputs.Select(input => supernet.ConnectionFactory(input, output));
                supernet.Connections.AddRange(connections);
            }
            foreach (var connTuple in borderOutputConnections)
            {
                var outputs = connTuple.Item2;
                var input = connTuple.Item1;
                var connections = outputs.Select(output => supernet.ConnectionFactory(input, output));
                supernet.Connections.AddRange(connections);
            }
        }
    }
}
