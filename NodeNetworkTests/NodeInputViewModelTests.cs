using System;
using System.Linq;
using DynamicData;
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
	        NodeViewModel nodeA = new NodeViewModel();
			nodeA.Outputs.Add(nodeAOutput);

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                CanBeRemovedByUser = false,
                IsSelected = true
            };
	        nodeB.Inputs.Add(nodeBInput);
			nodeB.Outputs.Add(nodeBOutput);

			NodeInputViewModel nodeCInput = new NodeInputViewModel
            {
                MaxConnections = 2
            };
            NodeViewModel nodeC = new NodeViewModel
            {
                IsSelected = true
            };
	        nodeC.Inputs.Add(nodeCInput);

			NodeViewModel nodeD = new NodeViewModel
            {
                IsSelected = true
            };

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[]{ nodeA, nodeB, nodeC, nodeD });

            Assert.IsTrue(nodeBInput.Connections.Count == 0);

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            var conBC = network.ConnectionFactory(nodeCInput, nodeBOutput);
            network.Connections.Add(conAB);
            network.Connections.Add(conBC);

            Assert.IsTrue(Enumerable.SequenceEqual(nodeBInput.Connections.Items, new[]{conAB}));

            network.Connections.Remove(conAB);

            Assert.IsTrue(nodeBInput.Connections.Count == 0);
            
            var conAC = network.ConnectionFactory(nodeCInput, nodeAOutput);
            network.Connections.Add(conAC);

            Assert.IsTrue(Enumerable.SequenceEqual(nodeCInput.Connections.Items, new[] { conBC, conAC }));
        }

        [TestMethod]
        public void TestHideEditorIfConnected()
        {
            TestableOutput output = new TestableOutput();
            TestableInput input = new TestableInput();
			
			var outputNode = new NodeViewModel();
			outputNode.Outputs.Add(output);

			var inputNode = new NodeViewModel();
			inputNode.Inputs.Add(input);

	        NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[] { outputNode, inputNode });

			input.HideEditorIfConnected = true;
            Assert.IsTrue(input.IsEditorVisible);

            network.Connections.Add(network.ConnectionFactory(input, output));

            Assert.IsFalse(input.IsEditorVisible);
        }

        [TestMethod]
        public void TestCreatePendingConnection()
        {
            TestableInput input = new TestableInput();

	        var node = new NodeViewModel();
			node.Inputs.Add(input);

			NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node);

            Assert.AreEqual(null, network.PendingConnection);

            input.CreatePendingConnection_public();

            Assert.AreEqual(input, network.PendingConnection.Input);
            Assert.IsTrue(network.PendingConnection.InputIsLocked);
        }

        [TestMethod]
        public void TestPreviewAndFinishPendingConnection()
        {
			TestableOutput output = new TestableOutput();
	        TestableInput input = new TestableInput();

	        var outputNode = new NodeViewModel();
	        outputNode.Outputs.Add(output);

	        var inputNode = new NodeViewModel();
	        inputNode.Inputs.Add(input);

	        NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[] { outputNode, inputNode });

			output.CreatePendingConnection_public();
            input.SetConnectionPreview_public(true);

            Assert.AreEqual(input, network.PendingConnection.Input);

            input.FinishPendingConnection_public();

            Assert.AreEqual(null, network.PendingConnection);

            Assert.AreEqual(1, network.Connections.Count);
            Assert.AreEqual(input, network.Connections.Items.First().Input);
            Assert.AreEqual(output, network.Connections.Items.First().Output);
        }
    }
}
