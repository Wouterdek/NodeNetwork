using System.Collections.Generic;
using System.Linq;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit
{
    /// <summary>
    /// This class is a collection of various graph algoritms.
    /// </summary>
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
        /// A loop is a connection sequence that starts and ends at the same node.
        /// </summary>
        /// <param name="network">the network to search for loops.</param>
        /// <returns>an enumeration of connections involved in loops</returns>
        public static IEnumerable<ConnectionViewModel> FindLoops(NetworkViewModel network)
        {
            Stack<NodeViewModel> nodesToCheck = new Stack<NodeViewModel>(network.Nodes.Items);
            Dictionary<NodeViewModel, NodeState> nodeStates = new Dictionary<NodeViewModel, NodeState>(nodesToCheck.Count);

            while (nodesToCheck.Count > 0)
            {
                NodeViewModel currentNode = nodesToCheck.Peek();

                NodeState state;
                if (!nodeStates.TryGetValue(currentNode, out state))
                {
                    state = NodeState.Ready;
                }

                if (state == NodeState.Error)
                {
                    nodesToCheck.Pop();
                    continue;
                }

                ConnectionViewModel recursiveConnection = FindLoops(nodeStates, currentNode);
                if (recursiveConnection != null)
                {
                    yield return recursiveConnection;
                }

                nodesToCheck.Pop();
            }
        }

        private static ConnectionViewModel FindLoops(Dictionary<NodeViewModel, NodeState> nodeStates, NodeViewModel node)
        {
            nodeStates[node] = NodeState.Busy;

            //Get the nodes connected to the inputs of node and check their state
            //If they are Ready, check them recursively.
            //If they are Busy, we found recursion.
            //If they are Error, the node was already found to be part of a loop and so we ignore it.
            List<ConnectionViewModel> nodesToCheck = new List<ConnectionViewModel>();
            foreach (NodeInputViewModel input in node.Inputs.Items)
            {
                foreach (ConnectionViewModel con in input.Connections.Items)
                {
                    NodeViewModel connectedNode = con.Output.Parent;
                    if (!nodeStates.TryGetValue(connectedNode, out var connectedNodeState))
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
                ConnectionViewModel result = FindLoops(nodeStates, currentNode);
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
        /// A call to FindLoops is recommended before using this function
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
                IEnumerable<NodeViewModel> inputNodes = startingNode.Inputs.Items.SelectMany(i => i.Connections.Items).Select(c => c.Output.Parent);
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
                IEnumerable<NodeViewModel> outputNodes = startingNode.Outputs.Items.SelectMany(i => i.Connections.Items).Select(c => c.Input.Parent);
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
        /// A call to FindLoops is recommended before using this function
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
            return FindStartingNodes(network.Nodes.Items);
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
                foreach (NodeInputViewModel input in cur.Inputs.Items)
                {
                    if (input.Connections.Items.Any(c => nodes.Contains(c.Output.Parent)))
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
        /// This method uses breadth-first search and keeps track of visited nodes, so it can handle networks with loops.
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

                foreach (NodeInputViewModel input in node.Inputs.Items)
                {
                    foreach (NodeViewModel connectedNode in input.Connections.Items.Select(c => c.Output.Parent))
                    {
                        if (visitedNodes.Add(connectedNode))
                        {
                            nodeQueue.Enqueue(connectedNode);
                        }
                    }
                }
                foreach (NodeOutputViewModel output in node.Outputs.Items)
                {
                    foreach (NodeViewModel connectedNode in output.Connections.Items.Select(c => c.Input.Parent))
                    {
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
        /// Because this method uses FindConnectedNodes, it is capable of handling networks with loops.
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
        /// Returns true if the given set of nodes form continuous subgraphs.
        /// The given set of nodes is split into subgraphs based on the connections between the nodes.
        /// If for each subgraph it is true that all nodes of the subgraph are in the provided set, then true is returned.
        /// Otherwise false is returned.
        /// Because this method uses FindSubGraphs, it is capable of handling networks with loops.
        /// </summary>
        public static bool IsContinuousSubGraphSet(HashSet<NodeViewModel> nodesInSubGraphSet)
        {
            return FindSubGraphs(FindStartingNodes(nodesInSubGraphSet))
                .All(subGraph => IsContinuousSubGroup(nodesInSubGraphSet, subGraph));
        }
        public static bool IsContinuousSubGraphSet(IEnumerable<NodeViewModel> nodesInSubGraphSet) 
            => IsContinuousSubGraphSet(new HashSet<NodeViewModel>(nodesInSubGraphSet));

        private static bool IsContinuousSubGroup(HashSet<NodeViewModel> groupNodesSet, IEnumerable<NodeViewModel> subGraphStartingNodes)
        {
            Queue<NodeViewModel> queue = new Queue<NodeViewModel>(subGraphStartingNodes);
            HashSet<NodeViewModel> visitedNodes = new HashSet<NodeViewModel>(queue);

            //Transitions from inside to outside to inside the group are not allowed in a continuous group.
            //Since we start from the starting nodes of the current subgroup, which are inside, 
            //we only need to check for a transitions from outside to inside.
            while (queue.Count > 0)
            {
                NodeViewModel cur = queue.Dequeue();

                foreach (NodeOutputViewModel output in cur.Outputs.Items)
                {
                    foreach (ConnectionViewModel con in output.Connections.Items)
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
