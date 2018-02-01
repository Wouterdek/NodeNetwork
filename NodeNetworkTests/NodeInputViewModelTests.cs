using System;
using System.Linq;
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
        public void TestConnections()
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

            NodeInputViewModel nodeCInput = new NodeInputViewModel
            {
                MaxConnections = 2
            };
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

            Assert.IsTrue(nodeBInput.Connections.IsEmpty);

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            var conBC = network.ConnectionFactory(nodeCInput, nodeBOutput);
            network.Connections.Add(conAB);
            network.Connections.Add(conBC);

            Assert.IsTrue(Enumerable.SequenceEqual(nodeBInput.Connections, new[]{conAB}));

            network.Connections.Remove(conAB);

            Assert.IsTrue(nodeBInput.Connections.IsEmpty);
            
            var conAC = network.ConnectionFactory(nodeCInput, nodeAOutput);
            network.Connections.Add(conAC);

            Assert.IsTrue(Enumerable.SequenceEqual(nodeCInput.Connections, new[] { conBC, conAC }));
        }

        [TestMethod]
        public void TestHideEditorIfConnected()
        {

        }
    }
}
