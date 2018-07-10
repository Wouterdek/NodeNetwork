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

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            network.Connections.Add(conAB);

            Assert.IsTrue(nodeAOutput.Connections.SequenceEqual(new[]
            {
                conAB
            }));

            var conAC = network.ConnectionFactory(nodeCInput, nodeAOutput);
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

        [TestMethod]
        public void TestCreatePendingConnection()
        {
            TestableOutput output = new TestableOutput();
            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = {
                    new NodeViewModel
                    {
                        Outputs = { output }
                    }
                }
            };

            Assert.AreEqual(null, network.PendingConnection);

            output.CreatePendingConnection_public();

            Assert.AreEqual(output, network.PendingConnection.Output);
            Assert.IsTrue(network.PendingConnection.OutputIsLocked);
        }

        [TestMethod]
        public void TestPreviewAndFinishPendingConnection()
        {
            TestableOutput output = new TestableOutput();
            TestableInput input = new TestableInput();
            NetworkViewModel network = new NetworkViewModel
            {
                Nodes = {
                    new NodeViewModel
                    {
                        Outputs = { output }
                    },
                    new NodeViewModel
                    {
                        Inputs = { input }
                    }
                }
            };
            
            input.CreatePendingConnection_public();
            output.SetConnectionPreview_public(true);

            Assert.AreEqual(output, network.PendingConnection.Output);

            output.FinishPendingConnection_public();

            Assert.AreEqual(null, network.PendingConnection);

            Assert.AreEqual(1, network.Connections.Count);
            Assert.AreEqual(input, network.Connections[0].Input);
            Assert.AreEqual(output, network.Connections[0].Output);
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
