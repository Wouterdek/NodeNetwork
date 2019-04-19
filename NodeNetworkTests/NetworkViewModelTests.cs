﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;
using ReactiveUI.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void TestAddConnectionShouldUpdateInputAndOutputConnLists()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel
            {
                Outputs = { output }
            };

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input }
            };

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            CollectionAssert.AreEqual(new []{ conn }, input.Connections.ToArray());
            CollectionAssert.AreEqual(new[] { conn }, output.Connections.ToArray());

            network.Connections.Clear();

            CollectionAssert.AreEqual(new ConnectionViewModel[0], input.Connections.ToArray());
            CollectionAssert.AreEqual(new ConnectionViewModel[0], output.Connections.ToArray());
        }

        [TestMethod]
        public void TestDeleteOutputShouldRemoveConnections()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel
            {
                Outputs = { output }
            };

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input }
            };

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Contains(conn));
            node1.Outputs.Remove(output);
            Assert.IsFalse(network.Connections.Contains(conn));

            node1.Outputs.Add(output);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Contains(conn));
            node1.Outputs.Clear();
            Assert.IsFalse(network.Connections.Contains(conn));
        }

        [TestMethod]
        public void TestDeleteInputShouldRemoveConnections()
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel
            {
                Outputs = { output }
            };

            NodeInputViewModel input = new NodeInputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input }
            };

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn = network.ConnectionFactory(input, output);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Contains(conn));
            node2.Inputs.Remove(input);
            Assert.IsFalse(network.Connections.Contains(conn));

            node2.Inputs.Add(input);
            network.Connections.Add(conn);

            Assert.IsTrue(network.Connections.Contains(conn));
            node2.Inputs.Clear();
            Assert.IsFalse(network.Connections.Contains(conn));
        }

        [TestMethod]
        public void TestDeleteNodeShouldRemoveConnections()
        {
            NodeInputViewModel input1 = new NodeInputViewModel();
            NodeOutputViewModel output1 = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel
            {
                Inputs = { input1 },
                Outputs = { output1 }
            };

            NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input2 },
                Outputs = { output2 }
            };

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var conn1 = network.ConnectionFactory(input1, output2);
            var conn2 = network.ConnectionFactory(input2, output1);
            network.Connections.Add(conn1);
            network.Connections.Add(conn2);

            Assert.IsTrue(network.Connections.Contains(conn1));
            Assert.IsTrue(network.Connections.Contains(conn2));
            network.Nodes.Remove(node1);
            Assert.IsFalse(network.Connections.Contains(conn1));
            Assert.IsFalse(network.Connections.Contains(conn2));

            network.Nodes.AddRange(new []{node1, node2});
            network.Connections.AddRange(new[] { conn1, conn2 });

            Assert.IsTrue(network.Connections.Contains(conn1));
            Assert.IsTrue(network.Connections.Contains(conn2));
            network.Nodes.Clear();
            Assert.IsFalse(network.Connections.Contains(conn1));
            Assert.IsFalse(network.Connections.Contains(conn2));
        }

        [TestMethod]
        public void TestValidateAfterConnectionsAllUpdated()
        {
            NodeInputViewModel input1 = new NodeInputViewModel();
            NodeOutputViewModel output1 = new NodeOutputViewModel();
            NodeViewModel node1 = new NodeViewModel
            {
                Inputs = { input1 },
                Outputs = { output1 }
            };

            NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input2 },
                Outputs = { output2 }
            };

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
            NodeViewModel node1 = new NodeViewModel
            {
                Inputs = { input1 },
                Outputs = { output1 }
            };

            NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input2 },
                Outputs = { output2 }
            };

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
            NodeViewModel node1 = new NodeViewModel
            {
                Inputs = { input1 },
                Outputs = { output1 }
            };

            NodeInputViewModel input2 = new NodeInputViewModel();
            NodeOutputViewModel output2 = new NodeOutputViewModel();
            NodeViewModel node2 = new NodeViewModel
            {
                Inputs = { input2 },
                Outputs = { output2 }
            };

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
		    using (TestUtils.WithScheduler(ImmediateScheduler.Instance))
		    {
			    var input1 = new ValueListNodeInputViewModel<string>();
				NodeViewModel node1 = new NodeViewModel
			    {
				    Inputs = {input1}
			    };

			    var output2 = new ValueNodeOutputViewModel<string>
			    {
				    Value = Observable.Return("Test")
			    };
			    NodeViewModel node2 = new NodeViewModel
			    {
				    Outputs = {output2}
			    };

			    NetworkViewModel network = new NetworkViewModel();

			    network.Nodes.Add(node1);
			    network.Nodes.Add(node2);

			    var conn1 = network.ConnectionFactory(input1, output2);
			    network.Connections.Add(conn1);

			    CollectionAssert.AreEqual(new[] {"Test"}, input1.Values.ToArray());
				
			    network.Connections.Remove(conn1);

			    CollectionAssert.AreEqual(new string[0], input1.Values.ToArray());
		    }
	    }
    }
}
