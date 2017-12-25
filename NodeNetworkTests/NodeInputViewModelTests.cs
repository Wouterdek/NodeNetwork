using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.ViewModels;

namespace NodeNetworkTests
{
    [TestClass]
    public class NodeInputViewModelTests
    {
        [TestMethod]
        public void TestPortParent()
        {
            NodeInputViewModel input = new NodeInputViewModel();
            Assert.AreEqual(input, input.Port.Parent);
        }

        [TestMethod]
        public void TestConnection()
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

            Assert.AreEqual(null, nodeBInput.Connection);

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            var conBC = network.ConnectionFactory(nodeCInput, nodeBOutput);
            network.Connections.Add(conAB);
            network.Connections.Add(conBC);

            Assert.AreEqual(conAB, nodeBInput.Connection);

            network.Connections.Remove(conAB);

            Assert.AreEqual(null, nodeBInput.Connection);
        }

        [TestMethod]
        public void TestHideEditorIfConnected()
        {

        }
    }
}
