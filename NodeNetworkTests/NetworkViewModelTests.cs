using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetworkTests
{
    [TestClass]
    public class NetworkViewModelTests
    {
        [TestMethod]
        public void TestChildNodeParent()
        {
            NodeViewModel node = new NodeViewModel();
            Assert.AreEqual(null, node.Parent);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node);
            Assert.AreEqual(network, node.Parent);

            network.Nodes.Remove(node);
            Assert.AreEqual(null, node.Parent);

            network.Nodes.Add(node);
            network.Nodes.Clear();
            Assert.AreEqual(null, node.Parent);
        }

        [TestMethod]
        public void TestDeleteSelectedNodes()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel
            {
                Outputs = { nodeAOutput }
            };
            
            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                CanBeRemovedByUser = false,
                IsSelected = true,
                Inputs = { nodeBInput },
                Outputs = { nodeBOutput }
            };

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
            NodeViewModel nodeC = new NodeViewModel
            {
                Inputs = { nodeCInput },
                IsSelected = true
            };

            NodeViewModel nodeD = new NodeViewModel
            {
                IsSelected = true
            };

            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = { nodeA, nodeB, nodeC, nodeD }
            };

            network.Connections.Add(network.ConnectionFactory(nodeBInput, nodeAOutput));
            network.Connections.Add(network.ConnectionFactory(nodeCInput, nodeBOutput));

            Observable.Return(Unit.Default).InvokeCommand(network.DeleteSelectedNodes);

            Assert.AreEqual(1, network.Connections.Count);
            Assert.IsTrue(network.Nodes.SequenceEqual(new []{nodeA, nodeB}));
        }

        [TestMethod]
        public void TestSelectedNodes()
        {
            NetworkViewModel network = new NetworkViewModel();

            NodeViewModel nodeA = new NodeViewModel();
            NodeViewModel nodeB = new NodeViewModel();

            network.Nodes.Add(nodeA);
            network.Nodes.Add(nodeB);

            Assert.AreEqual(0, network.SelectedNodes.Count);

            nodeA.IsSelected = true;

            Assert.AreEqual(1, network.SelectedNodes.Count);

            nodeB.IsSelected = true;

            Assert.AreEqual(2, network.SelectedNodes.Count);

            nodeA.IsSelected = false;
            nodeB.IsSelected = false;

            Assert.AreEqual(0, network.SelectedNodes.Count);
        }

        [TestMethod]
        public void TestCutLine()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel
            {
                Outputs = { nodeAOutput }
            };

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                CanBeRemovedByUser = false,
                IsSelected = true,
                Inputs = { nodeBInput },
                Outputs = { nodeBOutput }
            };

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
            NodeViewModel nodeC = new NodeViewModel
            {
                Inputs = { nodeCInput },
                IsSelected = true
            };

            NodeViewModel nodeD = new NodeViewModel
            {
                IsSelected = true
            };

            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = { nodeA, nodeB, nodeC, nodeD }
            };

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            var conBC = network.ConnectionFactory(nodeCInput, nodeBOutput);
            network.Connections.Add(conAB);
            network.Connections.Add(conBC);

            network.StartCut();
            network.CutLine.IntersectingConnections.Add(conAB);
            network.FinishCut();

            Assert.IsTrue(network.Connections.SequenceEqual(new []{ conBC }));
            Assert.IsFalse(network.CutLine.IsVisible);
        }

        [TestMethod]
        public void TestRectangleSelection()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel
            {
                Outputs = { nodeAOutput }
            };

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                CanBeRemovedByUser = false,
                IsSelected = true,
                Inputs = { nodeBInput },
                Outputs = { nodeBOutput }
            };

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
            NodeViewModel nodeC = new NodeViewModel
            {
                Inputs = { nodeCInput },
                IsSelected = true
            };

            NodeViewModel nodeD = new NodeViewModel
            {
                IsSelected = true
            };

            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = { nodeA, nodeB, nodeC, nodeD }
            };

            network.StartRectangleSelection();
            network.SelectionRectangle.IntersectingNodes.Add(nodeA);
            network.SelectionRectangle.IntersectingNodes.Add(nodeD);
            network.FinishRectangleSelection();

            Assert.IsTrue(network.SelectedNodes.SequenceEqual(new[] { nodeA, nodeD }));
        }
    }
}
