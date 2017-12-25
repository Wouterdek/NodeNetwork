using System.Collections.Generic;
using System.Linq;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit
{
    public class GraphAlgorithms
    {
        /*
         * Algorithm:
         *  1. Pick next ready node from the nodes-to-check list, set as current
         *  
         *  a. Mark current node as busy
         *  b. For each input of the current node, find the connected node
         *  c. Check connected node state
         *      -> if ready
         *          => set connected node as current and goto a
         *      -> if busy
         *          => recursion found!
         *          => mark all busy nodes as broken!
         *          => mark all nodes connected to outputs of broken nodes as broken
         *          => goto 1
         *  d. Mark current node as ready
         *  
         *  2. Goto 1
         */

        private enum NodeState
        {
            Ready,
            Busy,
            Error
        }

        /// <summary>
        /// Searches for loops in a network.
        /// </summary>
        /// <param name="network">the network to search loops in.</param>
        /// <returns>an enumeration of connections involved in loops</returns>
        public static IEnumerable<ConnectionViewModel> FindRecursion(NetworkViewModel network)
        {
            List<NodeViewModel> nodesToCheck = network.Nodes.ToList();
            Dictionary<NodeViewModel, NodeState> nodeStates = new Dictionary<NodeViewModel, NodeState>(nodesToCheck.Count);

            while (nodesToCheck.Count > 0)
            {
                NodeViewModel currentNode = nodesToCheck[0];
                NodeState state;
                if (!nodeStates.TryGetValue(currentNode, out state))
                {
                    state = NodeState.Ready;
                }

                if (state == NodeState.Error)
                {
                    nodesToCheck.Remove(currentNode);
                    continue;
                }

                ConnectionViewModel recursiveConnection = FindRecursion(network, nodeStates, currentNode);
                if (recursiveConnection != null)
                { //Should we keep testing?
                    yield return recursiveConnection;
                }

                nodesToCheck.Remove(currentNode);
            }
        }

        private static ConnectionViewModel FindRecursion(NetworkViewModel network, Dictionary<NodeViewModel, NodeState> nodeStates, NodeViewModel node)
        {
            nodeStates[node] = NodeState.Busy;

            List<ConnectionViewModel> nodesToCheck = new List<ConnectionViewModel>();
            foreach (NodeInputViewModel input in node.Inputs)
            {
                ConnectionViewModel con = input.Connection;
                if (con != null)
                {
                    NodeViewModel connectedNode = con.Output.Parent;
                    NodeState connectedNodeState;
                    if (!nodeStates.TryGetValue(connectedNode, out connectedNodeState))
                    {
                        connectedNodeState = NodeState.Ready;
                    }

                    if (connectedNodeState == NodeState.Ready)
                    {
                        nodesToCheck.Add(con);
                    }
                    else if (connectedNodeState == NodeState.Busy)
                    {
                        //Found recursion!
                        List<NodeViewModel> keys = new List<NodeViewModel>(nodeStates.Keys);
                        foreach (NodeViewModel cur in keys)
                        {
                            if (nodeStates[cur] == NodeState.Busy)
                            {
                                nodeStates[cur] = NodeState.Error;
                            }
                        }
                        return con;
                    }
                    else if (connectedNodeState == NodeState.Error)
                    {
                        //connected node is already marked with error state, no further action required
                    }
                }
            }

            foreach (ConnectionViewModel con in nodesToCheck)
            {
                NodeViewModel currentNode = con.Output.Parent;
                ConnectionViewModel result = FindRecursion(network, nodeStates, currentNode);
                if (result != null)
                {
                    return result;
                }
            }
            nodeStates[node] = NodeState.Ready;
            return null;
        }

        /// <summary>
        /// Returns the nodes connected to the starting node, then the nodes connected to those nodes, ... and so on.
        /// If the subgraph that contains the starting nodes has a loop, then this function will keep producing the values in the loop.
        /// A call to FindRecursion is recommended before using this function
        /// </summary>
        /// <param name="startingNode">The node from which to branch out</param>
        /// <param name="includeInputs">Include nodes connected through node inputs?</param>
        /// <param name="includeOutputs">Include nodes connected through node outputs?</param>
        /// <param name="includeSelf">Include the starting node? (will be first)</param>
        /// <returns>An enumeration of the nodes connected to the starting node.</returns>
        public static IEnumerable<NodeViewModel> GetConnectedNodesTunneling(NodeViewModel startingNode, bool includeInputs = true, bool includeOutputs = false, bool includeSelf = false)
        {
            if (includeSelf)
            {
                yield return startingNode;
            }

            if (includeInputs)
            {
                IEnumerable<NodeViewModel> inputNodes = startingNode.Inputs.Select(i => i.Connection).Where(c => c != null).Select(c => c.Output.Parent);
                foreach (NodeViewModel nodeVM in inputNodes)
                {
                    foreach (NodeViewModel subNodeVM in GetConnectedNodesTunneling(nodeVM, includeInputs, includeOutputs, true))
                    {
                        if (subNodeVM != startingNode)
                        {
                            yield return subNodeVM;
                        }
                    }
                }
            }

            if (includeOutputs)
            {
                IEnumerable<NodeViewModel> outputNodes = startingNode.Outputs.SelectMany(i => i.Connections).Select(c => c.Input.Parent);
                foreach (NodeViewModel nodeVM in outputNodes)
                {
                    foreach (NodeViewModel subNodeVM in GetConnectedNodesTunneling(nodeVM, includeInputs, includeOutputs, true))
                    {
                        if (subNodeVM != startingNode)
                        {
                            yield return subNodeVM;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Similar to GetConnectedNodesTunneling, but returns the outermost nodes first.
        /// If the subgraph that contains the starting nodes has a loop, then this function will never return.
        /// A call to FindRecursion is recommended before using this function
        /// </summary>
        public static IEnumerable<NodeViewModel> GetConnectedNodesBubbling(NodeViewModel startingNode, bool includeInputs = true, bool includeOutputs = false, bool includeSelf = false)
        {
            return GetConnectedNodesTunneling(startingNode, includeInputs, includeOutputs, includeSelf).Reverse();
        }

        /// <summary>
        /// Returns the starting nodes in the network.
        /// Starting nodes are nodes that do not have inputs connected to an output.
        /// </summary>
        /// <param name="network">The network to find starting nodes in</param>
        /// <returns>An enumerable of starting nodes</returns>
        public static IEnumerable<NodeViewModel> FindStartingNodes(NetworkViewModel network)
        {
            foreach (NodeViewModel node in network.Nodes)
            {
                bool hasInputConnection = false;
                foreach (NodeInputViewModel input in node.Inputs)
                {
                    if (input.Connection != null)
                    {
                        hasInputConnection = true;
                        break;
                    }
                }

                if (!hasInputConnection)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Returns the starting nodes in the node group.
        /// Starting nodes are nodes that do not have inputs connected to an output of a node in the group.
        /// </summary>
        public static IEnumerable<NodeViewModel> FindStartingNodes(IEnumerable<NodeViewModel> nodeGroup)
        {
            Queue<NodeViewModel> todo = new Queue<NodeViewModel>(nodeGroup);
            HashSet<NodeViewModel> nodes = new HashSet<NodeViewModel>(todo);

            while (todo.Count > 0)
            {
                NodeViewModel cur = todo.Dequeue();

                bool hasInputConnection = false;
                foreach (NodeInputViewModel input in cur.Inputs)
                {
                    NodeViewModel connectedNode = input.Connection?.Output.Parent;
                    if (connectedNode != null && nodes.Contains(connectedNode))
                    {
                        hasInputConnection = true;
                        break;
                    }
                }

                if (!hasInputConnection)
                {
                    yield return cur;
                }
            }
        }

        /// <summary>
        /// Takes the provided set of nodes and returns the nodes are connected to the source node, directly or indirectly.
        /// </summary>
        /// <param name="sourceNode">The node from which the search for connected nodes starts</param>
        /// <param name="nodes">
        /// The nodes to look for when searching. 
        /// If this set contains the sourcenode, the first item returned will be the source node.
        /// </param>
        /// <returns>An enumeration of connected nodes</returns>
        public static IEnumerable<NodeViewModel> FindConnectedNodes(NodeViewModel sourceNode, IEnumerable<NodeViewModel> nodes)
        {
            HashSet<NodeViewModel> nodesSet = new HashSet<NodeViewModel>(nodes);

            HashSet<NodeViewModel> visitedNodes = new HashSet<NodeViewModel>();

            Queue<NodeViewModel> nodeQueue = new Queue<NodeViewModel>();
            nodeQueue.Enqueue(sourceNode);

            while (nodeQueue.Count > 0)
            {
                NodeViewModel node = nodeQueue.Dequeue();

                if (nodesSet.Remove(node))
                {
                    yield return node;
                    if (nodesSet.Count == 0)
                    {
                        yield break;
                    }
                }

                foreach (NodeInputViewModel input in node.Inputs)
                {
                    NodeViewModel connectedNode = input.Connection?.Output.Parent;
                    if (connectedNode != null)
                    {
                        if (visitedNodes.Add(connectedNode))
                        {
                            nodeQueue.Enqueue(connectedNode);
                        }
                    }
                }
                foreach (NodeOutputViewModel output in node.Outputs)
                {
                    foreach (ConnectionViewModel con in output.Connections)
                    {
                        NodeViewModel connectedNode = con.Input.Parent;
                        if (visitedNodes.Add(connectedNode))
                        {
                            nodeQueue.Enqueue(connectedNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Takes the provided set of nodes and groups these nodes in sets that are connected, directly or indirectly.
        /// </summary>
        /// <param name="nodes">the nodes to group into sets</param>
        public static IEnumerable<IEnumerable<NodeViewModel>> FindSubGraphs(IEnumerable<NodeViewModel> nodes)
        {
            HashSet<NodeViewModel> nodesSet = new HashSet<NodeViewModel>(nodes);
            while (nodesSet.Count > 0)
            {
                NodeViewModel curNode = nodesSet.First();

                List<NodeViewModel> subGraphMembers = new List<NodeViewModel>(FindConnectedNodes(curNode, nodesSet));
                foreach (NodeViewModel subGraphMember in subGraphMembers)
                {
                    nodesSet.Remove(subGraphMember);
                }
                yield return subGraphMembers;
            }
        }

        /// <summary>
        /// Returns true if the given set of nodes are continuous.
        /// </summary>
        /// <param name="network"></param>
        /// <param name="nodesInGroup"></param>
        /// <returns></returns>
        public static bool IsContinuousGroup(NetworkViewModel network, IEnumerable<NodeViewModel> nodesInGroup)
        {
            HashSet<NodeViewModel> groupNodesSet = new HashSet<NodeViewModel>(nodesInGroup);
            foreach (IEnumerable<NodeViewModel> subGraphStartingNodes in FindSubGraphs(FindStartingNodes(groupNodesSet)))
            {
                if (!IsContinuousSubGroup(groupNodesSet, subGraphStartingNodes))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsContinuousSubGroup(HashSet<NodeViewModel> groupNodesSet, IEnumerable<NodeViewModel> subGraphStartingNodes)
        {
            Queue<NodeViewModel> queue = new Queue<NodeViewModel>(subGraphStartingNodes);
            HashSet<NodeViewModel> visitedNodes = new HashSet<NodeViewModel>(queue);

            //A transision from inside to outside to inside the group is not allowed in a continuous group.
            //Since we start from the starting nodes of the current subgroup, which are inside, 
            //we only need to check for a transision from outside to inside.
            while (queue.Count > 0)
            {
                NodeViewModel cur = queue.Dequeue();

                foreach (NodeOutputViewModel output in cur.Outputs)
                {
                    foreach (ConnectionViewModel con in output.Connections)
                    {
                        NodeViewModel connectedNode = con.Input.Parent;

                        if (groupNodesSet.Contains(connectedNode) && !groupNodesSet.Contains(cur))
                        {
                            //Found transision from outside to inside
                            return false;
                        }

                        if (!visitedNodes.Add(connectedNode))
                        {
                            continue;
                        }
                        queue.Enqueue(connectedNode);
                    }
                }
            }
            return true;
        }
    }
}
