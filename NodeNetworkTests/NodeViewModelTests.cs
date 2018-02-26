using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.ViewModels;

namespace NodeNetworkTests
{
    [TestClass]
    public class NodeViewModelTests
    {
        [TestMethod]
        public void TestCanBeRemovedByUser()
        {
            Assert.IsTrue(new NodeViewModel().CanBeRemovedByUser);
        }

        [TestMethod]
        public void TestInputParent()
        {
            NodeInputViewModel input = new NodeInputViewModel();
            Assert.AreEqual(null, input.Parent);

            NodeViewModel node = new NodeViewModel
            {
                Inputs = { input }
            };
            Assert.AreEqual(node, input.Parent);

            node.Inputs.Remove(input);
            Assert.AreEqual(null, input.Parent);
        }

        [TestMethod]
        public void TestOutputParent()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            Assert.AreEqual(null, output.Parent);

            NodeViewModel node = new NodeViewModel
            {
                Outputs = { output }
            };
            Assert.AreEqual(node, output.Parent);

            node.Outputs.Remove(output);
            Assert.AreEqual(null, output.Parent);
        }

        [TestMethod]
        public void TestNodeCollapse()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel
            {
                Outputs = { nodeAOutput }
            };

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeInputViewModel nodeBInput2 = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeOutputViewModel nodeBOutput2 = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                Inputs = { nodeBInput, nodeBInput2 },
                Outputs = { nodeBOutput, nodeBOutput2 }
            };

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
            NodeViewModel nodeC = new NodeViewModel
            {
                Inputs = { nodeCInput }
            };

            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = { nodeA, nodeB, nodeC }
            };

            network.Connections.Add(network.ConnectionFactory.CreateConnection(network, nodeBInput, nodeAOutput));
            network.Connections.Add(network.ConnectionFactory.CreateConnection(network, nodeCInput, nodeBOutput));

            nodeB.IsCollapsed = true;

            Assert.IsTrue(nodeB.VisibleInputs.SequenceEqual(new []{nodeBInput}));
            Assert.IsTrue(nodeB.VisibleOutputs.SequenceEqual(new[] { nodeBOutput }));
        }
    }
}
