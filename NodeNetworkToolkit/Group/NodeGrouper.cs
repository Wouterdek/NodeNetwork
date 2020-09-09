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
    public class NodeGrouper
    {
        public Func<NetworkViewModel, NodeViewModel> GroupNodeFactory { get; set; }
        public Func<NetworkViewModel> SubNetworkFactory { get; set; }

        public Func<NodeViewModel> EntranceNodeFactory { get; set; }
        public Func<NodeViewModel> ExitNodeFactory { get; set; }

        public Func<NodeViewModel, NodeViewModel, NodeViewModel, GroupIOBinding> IOBindingFactory { get; set; }

        public NodeViewModel MergeIntoGroup(NetworkViewModel network, IEnumerable<NodeViewModel> nodesToGroup)
        {
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

            var groupEntranceInputs = new Dictionary<NodeInputViewModel, NodeInputViewModel>();
            var groupExitOutputs = new Dictionary<NodeOutputViewModel, NodeOutputViewModel>();

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
                if (!groupEntranceInputs.ContainsKey(borderInCon.Input))
                {
                    groupEntranceInputs[borderInCon.Input] = ioBinding.AddNewEntranceInput(borderInCon.Output);
                }
            }

            foreach (var borderOutCon in borderOutputConnections)
            {
                if (!groupExitOutputs.ContainsKey(borderOutCon.Output))
                {
                    groupExitOutputs[borderOutCon.Output] = ioBinding.AddNewExitOutput(borderOutCon.Input);
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
                borderInputConnections.Select(con => network.ConnectionFactory(groupEntranceInputs[con.Input], con.Output)),
                borderOutputConnections.Select(con => network.ConnectionFactory(con.Input, groupExitOutputs[con.Output]))
            ));
            subnet.Connections.AddRange(Enumerable.Concat(
                borderInputConnections.Select(con => subnet.ConnectionFactory(con.Input, ioBinding.GetEntranceOutput(groupEntranceInputs[con.Input]))),
                borderOutputConnections.Select(con => subnet.ConnectionFactory(ioBinding.GetExitInput(groupExitOutputs[con.Output]), con.Output))
            ));

            return groupNode;
        }

        public void Ungroup()
        {

        }
    }
}
