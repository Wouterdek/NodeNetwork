using System;
using System.Linq;
using DynamicData;
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

            NodeViewModel node = new NodeViewModel();
			node.Inputs.Add(input);
            Assert.AreEqual(node, input.Parent);

            node.Inputs.Remove(input);
            Assert.AreEqual(null, input.Parent);

            node.Inputs.Add(input);
            Assert.AreEqual(node, input.Parent);
            node.Inputs.Clear();
            Assert.AreEqual(null, input.Parent);
        }

        [TestMethod]
        public void TestOutputParent()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            Assert.AreEqual(null, output.Parent);

            NodeViewModel node = new NodeViewModel();
			node.Outputs.Add(output);
            Assert.AreEqual(node, output.Parent);

            node.Outputs.Remove(output);
            Assert.AreEqual(null, output.Parent);

            node.Outputs.Add(output);
            Assert.AreEqual(node, output.Parent);
            node.Outputs.Clear();
            Assert.AreEqual(null, output.Parent);
        }

        [DataTestMethod]
        [DataRow(EndpointVisibility.Auto, true, true, false, true)]
        [DataRow(EndpointVisibility.AlwaysHidden, false, false, false, false)]
        [DataRow(EndpointVisibility.AlwaysVisible, true, true, true, true)]
        public void TestNodeCollapse(EndpointVisibility visibility, bool nonCollapsedNonConnectedVisible, bool nonCollapsedConnectedVisible, bool collapsedNonConnectedVisible, bool collapsedConnectedVisible)
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
	        NodeViewModel nodeA = new NodeViewModel();
			nodeA.Outputs.Add(nodeAOutput);

            NodeInputViewModel nodeBInput = new NodeInputViewModel { Visibility = visibility };
            NodeInputViewModel nodeBInput2 = new NodeInputViewModel { Visibility = visibility };
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel { Visibility = visibility };
            NodeOutputViewModel nodeBOutput2 = new NodeOutputViewModel { Visibility = visibility };
            NodeViewModel nodeB = new NodeViewModel();
			nodeB.Inputs.AddRange(new []{ nodeBInput, nodeBInput2 });
			nodeB.Outputs.AddRange(new []{ nodeBOutput, nodeBOutput2 });

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
            NodeViewModel nodeC = new NodeViewModel();
			nodeC.Inputs.Add(nodeCInput);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new[] { nodeA, nodeB, nodeC });

            network.Connections.Add(network.ConnectionFactory(nodeBInput, nodeAOutput));
            network.Connections.Add(network.ConnectionFactory(nodeCInput, nodeBOutput));

            var expectedInputSeq = Enumerable.Empty<Endpoint>();
            var expectedOutputSeq = Enumerable.Empty<Endpoint>();
            if (nonCollapsedConnectedVisible)
            {
                expectedInputSeq = expectedInputSeq.Concat(new[] { nodeBInput });
                expectedOutputSeq = expectedOutputSeq.Concat(new[] { nodeBOutput });
            }
            if (nonCollapsedNonConnectedVisible)
            {
                expectedInputSeq = expectedInputSeq.Concat(new[] { nodeBInput2 });
                expectedOutputSeq = expectedOutputSeq.Concat(new[] { nodeBOutput2 });
            }

            Assert.IsTrue(nodeB.VisibleInputs.Items.SequenceEqual(expectedInputSeq));
            Assert.IsTrue(nodeB.VisibleOutputs.Items.SequenceEqual(expectedOutputSeq));

            nodeB.IsCollapsed = true;

            expectedInputSeq = Enumerable.Empty<Endpoint>();
            expectedOutputSeq = Enumerable.Empty<Endpoint>();

            if (collapsedConnectedVisible)
            {
                expectedInputSeq = expectedInputSeq.Concat(new[] { nodeBInput });
                expectedOutputSeq = expectedOutputSeq.Concat(new[] { nodeBOutput });
            }
            if (collapsedNonConnectedVisible)
            {
                expectedInputSeq = expectedInputSeq.Concat(new[] { nodeBInput2 });
                expectedOutputSeq = expectedOutputSeq.Concat(new[] { nodeBOutput2 });
            }

            Assert.IsTrue(nodeB.VisibleInputs.Items.SequenceEqual(expectedInputSeq));
            Assert.IsTrue(nodeB.VisibleOutputs.Items.SequenceEqual(expectedOutputSeq));
        }
    }
}
