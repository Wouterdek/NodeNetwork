using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Kernel;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;
using ReactiveUI.Testing;

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
            Assert.AreEqual(network, node.Parent);
            network.Nodes.Clear();
            Assert.AreEqual(null, node.Parent);
        }

        [TestMethod]
        public void TestDeleteSelectedNodes()
        {
            NodeOutputViewModel nodeAOutput = new NodeOutputViewModel();
            NodeViewModel nodeA = new NodeViewModel();
			nodeA.Outputs.Add(nodeAOutput);
            
            NodeInputViewModel nodeBInput = new NodeInputViewModel();
            NodeOutputViewModel nodeBOutput = new NodeOutputViewModel();
            NodeViewModel nodeB = new NodeViewModel
            {
                CanBeRemovedByUser = false,
                IsSelected = true,
            };
			nodeB.Inputs.Add(nodeBInput);
			nodeB.Outputs.Add(nodeBOutput);

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
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
            network.Nodes.AddRange(new[] { nodeA, nodeB, nodeC, nodeD });

            network.Connections.Add(network.ConnectionFactory(nodeBInput, nodeAOutput));
            network.Connections.Add(network.ConnectionFactory(nodeCInput, nodeBOutput));
            
            Observable.Return(Unit.Default).InvokeCommand(network.DeleteSelectedNodes);

            Assert.AreEqual(1, network.Connections.Count);
            Assert.IsTrue(network.Nodes.Items.SequenceEqual(new []{nodeA, nodeB}));
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

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
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
            network.Nodes.AddRange(new[] { nodeA, nodeB, nodeC, nodeD });

            var conAB = network.ConnectionFactory(nodeBInput, nodeAOutput);
            var conBC = network.ConnectionFactory(nodeCInput, nodeBOutput);
            network.Connections.Add(conAB);
            network.Connections.Add(conBC);

            network.StartCut();
            network.CutLine.IntersectingConnections.Add(conAB);
            network.FinishCut();

            Assert.IsTrue(network.Connections.Items.SequenceEqual(new []{ conBC }));
            Assert.IsFalse(network.CutLine.IsVisible);
        }

        [TestMethod]
        public void TestRectangleSelection()
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

            NodeInputViewModel nodeCInput = new NodeInputViewModel();
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
            network.Nodes.AddRange(new[] { nodeA, nodeB, nodeC, nodeD });

            network.StartRectangleSelection();
            network.SelectionRectangle.IntersectingNodes.Add(nodeA);
            network.SelectionRectangle.IntersectingNodes.Add(nodeD);
            network.FinishRectangleSelection();

            Assert.IsTrue(network.SelectedNodes.Items.SequenceEqual(new[] { nodeA, nodeD }));
        }

        [TestMethod]
        public void TestAddConnectionShouldUpdateInputAndOutputConnLists()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel();
			node1.Outputs.Add(output);

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel();
			node2.Inputs.Add(input);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            CollectionAssert.AreEqual(new []{ conn }, input.Connections.Items.AsArray());
            CollectionAssert.AreEqual(new[] { conn }, output.Connections.Items.AsArray());

            network.Connections.Clear();

            CollectionAssert.AreEqual(new ConnectionViewModel[0], input.Connections.Items.AsArray());
            CollectionAssert.AreEqual(new ConnectionViewModel[0], output.Connections.Items.AsArray());
        }

        [TestMethod]
        public void TestDeleteOutputShouldRemoveConnections()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel();
			node1.Outputs.Add(output);

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel();
			node2.Inputs.Add(input);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Items.Contains(conn));
            node1.Outputs.Remove(output);
            Assert.IsFalse(network.Connections.Items.Contains(conn));

            node1.Outputs.Add(output);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Items.Contains(conn));
            node1.Outputs.Clear();
            Assert.IsFalse(network.Connections.Items.Contains(conn));
        }

        [TestMethod]
        public void TestDeleteInputShouldRemoveConnections()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel();
			node1.Outputs.Add(output);

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel();
			node2.Inputs.Add(input);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            Assert.IsTrue(input.Connections.Items.Contains(conn));
            Assert.IsTrue(network.Connections.Items.Contains(conn));
            node2.Inputs.Remove(input);
            Assert.IsFalse(input.Connections.Items.Contains(conn));
            Assert.IsFalse(network.Connections.Items.Contains(conn));

            node2.Inputs.Add(input);
            network.Connections.Add(conn);

            Assert.IsTrue(input.Connections.Items.Contains(conn));
            Assert.IsTrue(network.Connections.Items.Contains(conn));
            node2.Inputs.Clear();
            Assert.IsFalse(input.Connections.Items.Contains(conn));
            Assert.IsFalse(network.Connections.Items.Contains(conn));
        }

        [TestMethod]
        public void TestDeleteNodeShouldRemoveConnections()
        {
            NodeInputViewModel input1 = new NodeInputViewModel();
            NodeOutputViewModel output1 = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel();
			node1.Inputs.Add(input1);
			node1.Outputs.Add(output1);

            NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
	        NodeViewModel node2 = new NodeViewModel();
	        node2.Inputs.Add(input2);
	        node2.Outputs.Add(output2);

			NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn1 = network.ConnectionFactory(input1, output2);
            var conn2 = network.ConnectionFactory(input2, output1);
            network.Connections.Add(conn1);
            network.Connections.Add(conn2);

            Assert.IsTrue(network.Connections.Items.Contains(conn1));
            Assert.IsTrue(network.Connections.Items.Contains(conn2));
            network.Nodes.Remove(node1);
            Assert.IsFalse(network.Connections.Items.Contains(conn1));
            Assert.IsFalse(network.Connections.Items.Contains(conn2));

            network.Nodes.AddRange(new []{node1, node2});
            network.Connections.AddRange(new[] { conn1, conn2 });

            Assert.IsTrue(network.Connections.Items.Contains(conn1));
            Assert.IsTrue(network.Connections.Items.Contains(conn2));
            network.Nodes.Clear();
            Assert.IsFalse(network.Connections.Items.Contains(conn1));
            Assert.IsFalse(network.Connections.Items.Contains(conn2));
        }

        [TestMethod]
        public void TestValidateAfterConnectionsAllUpdated()
        {
            NodeInputViewModel input1 = new NodeInputViewModel();
            NodeOutputViewModel output1 = new NodeOutputViewModel();
	        NodeViewModel node1 = new NodeViewModel();
	        node1.Inputs.Add(input1);
	        node1.Outputs.Add(output1);

			NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel();
	        node2.Inputs.Add(input2);
	        node2.Outputs.Add(output2);

			NetworkViewModel network = new NetworkViewModel();
            network.Validator = n =>
            {
                if (GraphAlgorithms.FindLoops(network).Any())
                {
                    return new NetworkValidationResult(false, false, null);
                }
                return new NetworkValidationResult(true, true, null);
            };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            
            var conn1 = network.ConnectionFactory(input1, output2);
            network.Connections.Add(conn1);

            Assert.IsTrue(network.LatestValidation.IsValid);
            
            var conn2 = network.ConnectionFactory(input2, output1);
            network.Connections.Add(conn2);

            Assert.IsFalse(network.LatestValidation.IsValid);
            
            network.Connections.Remove(conn1);

            Assert.IsTrue(network.LatestValidation.IsValid);
        }

        [TestMethod]
        public void TestConnectionsUpdatedAfterPreexistingConnectionRemoved()
        {
            //Setup
            var scheduler = new TestScheduler();

            NodeInputViewModel input1 = new NodeInputViewModel();
            NodeOutputViewModel output1 = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel();
	        node1.Inputs.Add(input1);
	        node1.Outputs.Add(output1);

			NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel();
	        node2.Inputs.Add(input2);
	        node2.Outputs.Add(output2);

			NetworkViewModel network = new NetworkViewModel();

            var observable = network.ConnectionsUpdated; // Create observable before nodes/connections are added

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn1 = network.ConnectionFactory(input1, output2);
            network.Connections.Add(conn1);
            

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => network.Connections.Remove(conn1));
            var actual = scheduler.Start(() => observable, created: 0, subscribed: 0, disposed: 100); // But subscribe to it here

            //Assert
            var expected = new[]
            {
                ReactiveTest.OnNext(10, Unit.Default)
            };
            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [TestMethod]
        public void TestNestedObserving()
        {
            //Setup
            var scheduler = new TestScheduler();

			NodeInputViewModel input1 = new NodeInputViewModel();
	        NodeOutputViewModel output1 = new NodeOutputViewModel();
	        NodeViewModel node1 = new NodeViewModel();
	        node1.Inputs.Add(input1);
	        node1.Outputs.Add(output1);

	        NodeInputViewModel input2 = new NodeInputViewModel();
	        NodeOutputViewModel output2 = new NodeOutputViewModel();
	        NodeViewModel node2 = new NodeViewModel();
	        node2.Inputs.Add(input2);
	        node2.Outputs.Add(output2);

			NetworkViewModel network = new NetworkViewModel();

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn1 = network.ConnectionFactory(input1, output2);
            network.Connections.Add(conn1);

            var obs = network.ConnectionsUpdated;


            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => network.Connections.Remove(conn1));
            var actual = scheduler.Start(() => obs, created: 0, subscribed: 0, disposed: 100); // But subscribe to it here

            //Assert
            var expected = new[]
            {
                ReactiveTest.OnNext(10, Unit.Default)
            };
            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

	    [TestMethod]
	    public void TestListInputDisconnect()
	    {
			var input1 = new ValueListNodeInputViewModel<string>();
			NodeViewModel node1 = new NodeViewModel();
			node1.Inputs.Add(input1);

			var output2 = new ValueNodeOutputViewModel<string>
			{
				Value = Observable.Return("Test")
			};
			NodeViewModel node2 = new NodeViewModel();
			node2.Outputs.Add(output2);

			NetworkViewModel network = new NetworkViewModel();

			network.Nodes.Add(node1);
			network.Nodes.Add(node2);

			var conn1 = network.ConnectionFactory(input1, output2);
			network.Connections.Add(conn1);

			CollectionAssert.AreEqual(new[] {"Test"}, input1.Values.Items.AsArray());
				
			network.Connections.Remove(conn1);

			CollectionAssert.AreEqual(new string[0], input1.Values.Items.AsArray());
	    }
    }
}
