using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.ViewModels;

namespace NodeNetworkTests
{
    [TestClass]
    public class NodeOutputViewModelTests
    {
        [TestMethod]
        public void TestPortParent()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            Assert.AreEqual(output, output.Port.Parent);
        }

        [TestMethod]
        public void TestConnections()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel
            {
                Outputs = { nodeAOutput }
            };

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                Inputs = { nodeBInput }
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

            Assert.AreEqual(0, nodeAOutput.Connections.Count);

            var conAB = network.ConnectionFactory.CreateConnection(network, nodeBInput, nodeAOutput);
            network.Connections.Add(conAB);

            Assert.IsTrue(nodeAOutput.Connections.SequenceEqual(new[]
            {
                conAB
            }));

            var conAC = network.ConnectionFactory.CreateConnection(network, nodeCInput, nodeAOutput);
            network.Connections.Add(conAC);

            Assert.IsTrue(nodeAOutput.Connections.SequenceEqual(new []
            {
                conAB, conAC
            }));

            network.Connections.Remove(conAB);

            Assert.IsTrue(nodeAOutput.Connections.SequenceEqual(new[]
            {
                conAC
            }));

            network.Connections.Remove(conAC);

            Assert.AreEqual(0, nodeAOutput.Connections.Count);
        }
    }
}
