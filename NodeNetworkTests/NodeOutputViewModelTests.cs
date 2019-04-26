using System;
using System.Linq;
using DynamicData;
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
            NodeViewModel nodeA = new NodeViewModel();
			nodeA.Outputs.Add(nodeAOutput);

            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeViewModel nodeB = new NodeViewModel();
			nodeB.Inputs.Add(nodeBInput);

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
	        NodeViewModel nodeC = new NodeViewModel();
			nodeC.Inputs.Add(nodeCInput);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[]{ nodeA, nodeB, nodeC });

            Assert.AreEqual(0, nodeAOutput.Connections.Count);

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            network.Connections.Add(conAB);

            Assert.IsTrue(nodeAOutput.Connections.Items.SequenceEqual(new[]
            {
                conAB
            }));

            var conAC = network.ConnectionFactory(nodeCInput, nodeAOutput);
            network.Connections.Add(conAC);

            Assert.IsTrue(nodeAOutput.Connections.Items.SequenceEqual(new []
            {
                conAB, conAC
            }));

            network.Connections.Remove(conAB);

            Assert.IsTrue(nodeAOutput.Connections.Items.SequenceEqual(new[]
            {
                conAC
            }));

            network.Connections.Remove(conAC);

            Assert.AreEqual(0, nodeAOutput.Connections.Count);
        }

        [TestMethod]
        public void TestCreatePendingConnection()
        {
            TestableOutput output = new TestableOutput();
	        var node = new NodeViewModel();
			node.Outputs.Add(output);

			NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node);

            Assert.AreEqual(null, network.PendingConnection);

            output.CreatePendingConnection_public();

            Assert.AreEqual(output, network.PendingConnection.Output);
            Assert.IsTrue(network.PendingConnection.OutputIsLocked);
        }

        [TestMethod]
        public void TestPreviewAndFinishPendingConnection()
        {
            TestableOutput output = new TestableOutput();
			var outputNode = new NodeViewModel();
			outputNode.Outputs.Add(output);

            TestableInput input = new TestableInput();
	        var inputNode = new NodeViewModel();
	        inputNode.Inputs.Add(input);

			NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[] { outputNode, inputNode });
            
            input.CreatePendingConnection_public();
            output.SetConnectionPreview_public(true);

            Assert.AreEqual(output, network.PendingConnection.Output);

            output.FinishPendingConnection_public();

            Assert.AreEqual(null, network.PendingConnection);

            Assert.AreEqual(1, network.Connections.Count);
            Assert.AreEqual(input, network.Connections.Items.First().Input);
            Assert.AreEqual(output, network.Connections.Items.First().Output);
        }
    }

    internal class TestableOutput : NodeOutputViewModel
    {
        public void CreatePendingConnection_public()
        {
            CreatePendingConnection();
        }

        public void SetConnectionPreview_public(bool previewActive)
        {
            SetConnectionPreview(previewActive);
        }

        public void FinishPendingConnection_public()
        {
            FinishPendingConnection();
        }
    }

    internal class TestableInput : NodeInputViewModel
    {
        public void CreatePendingConnection_public()
        {
            CreatePendingConnection();
        }

        public void SetConnectionPreview_public(bool previewActive)
        {
            SetConnectionPreview(previewActive);
        }

        public void FinishPendingConnection_public()
        {
            FinishPendingConnection();
        }
    }
}
