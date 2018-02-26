using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NodeNetwork;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI.Testing;

namespace NodeNetworkTests
{
    [TestClass]
    public class SerializationTests
    {
        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestConnection(SerializationFramework framework)
        {
            NodeOutputViewModel output = new NodeOutputViewModel();
            NodeInputViewModel input = new NodeInputViewModel();
            NetworkViewModel network = new NetworkViewModel
            {
                Nodes =
                {
                    new NodeViewModel{Outputs = { output }},
                    new NodeViewModel{Inputs = { input }}
                }
            };

            ConnectionViewModel vm = new ConnectionViewModel(network, input, output)
            {
                CanBeRemovedByUser = false,
                IsHighlighted = true,
                IsInErrorState = true,
            };

            var clone = Clone(vm, framework);

            AssertConnectionsEqual(vm, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestCutLine(SerializationFramework framework)
        {
            CutLineViewModel vm = new CutLineViewModel
            {
                StartPoint = new Point(1, 2),
                EndPoint = new Point(3, 4),
                IsVisible = true,
                IntersectingConnections = { new ConnectionViewModel(new NetworkViewModel(), new NodeInputViewModel(), new NodeOutputViewModel())}
            };

            var clone = Clone(vm, framework);

            AssertCutLineEqual(vm, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestErrorMessage(SerializationFramework framework)
        {
            ErrorMessageViewModel vm = new ErrorMessageViewModel("Test");
            var clone = Clone(vm, framework);
            AssertErrorMessagesEqual(vm, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestNetwork(SerializationFramework framework)
        {
            NodeInputViewModel input = new NodeInputViewModel
            {
                Name = "Input"
            };
            NodeOutputViewModel output = new NodeOutputViewModel
            {
                Name = "Output"
            };
            NetworkViewModel network = new NetworkViewModel
            {
                Nodes =
                {
                    new NodeViewModel
                    {
                        Name = "A",
                        Inputs = { input},
                        IsSelected = true
                    },
                    new NodeViewModel
                    {
                        Name = "B",
                        Outputs = { output}
                    }
                },
                PendingNode = new NodeViewModel {Name = "Pending"}
            };
            network.PendingConnection = new PendingConnectionViewModel(network)
            {
                Output = output,
                OutputIsLocked = true,
                LooseEndPoint = new Point(10, 12)
            };
            network.CutLine.IsVisible = true;
            network.CutLine.StartPoint = new Point(10, 10);
            network.CutLine.EndPoint = new Point(20, 20);
            network.Connections.Add(network.ConnectionFactory.CreateConnection(network, input, output));

            NetworkViewModel clone = Clone(network, framework);

            AssertNetworksEqual(network, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestSelectedNodes(SerializationFramework framework)
        {
            using (TestUtils.WithScheduler(ImmediateScheduler.Instance))
            {
                NetworkViewModel network = new NetworkViewModel
                {
                    Nodes =
                    {
                        new NodeViewModel
                        {
                            IsSelected = true
                        },
                        new NodeViewModel()
                    },
                };
                NetworkViewModel clone = Clone(network, framework);

                network.Nodes[1].IsSelected = true;
                clone.Nodes[1].IsSelected = true;
                Assert.AreEqual(2, clone.SelectedNodes.Count);
                clone.Nodes.Add(new NodeViewModel { IsSelected = true });
                Assert.AreEqual(3, clone.SelectedNodes.Count);
            }
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestEditor(SerializationFramework framework)
        {
            NodeEndpointEditorViewModel vm = new NodeEndpointEditorViewModel();
            Endpoint e = new NodeInputViewModel { Editor = vm };

            var clone = Clone(vm, framework);

            AssertEditorsEqual(vm, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestInput(SerializationFramework framework)
        {
            NodeInputViewModel input = new NodeInputViewModel
            {
                Name = "Input",
                HideEditorIfConnected = false,
                MaxConnections = 42,
                PortPosition = PortPosition.Left
            };

            NodeViewModel node = new NodeViewModel
            {
                Inputs = { input }
            };

            NodeInputViewModel clone = Clone(input, framework);

            AssertInputsEqual(input, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestOutput(SerializationFramework framework)
        {
            NodeOutputViewModel output = new NodeOutputViewModel
            {
                Name = "Output",
                MaxConnections = 42,
                PortPosition = PortPosition.Left
            };

            NodeViewModel node = new NodeViewModel
            {
                Outputs = { output }
            };

            NodeOutputViewModel clone = Clone(output, framework);

            AssertOutputsEqual(clone, output);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestNode(SerializationFramework framework)
        {
            NodeViewModel node = new NodeViewModel
            {
                Name = "Test",
                IsSelected = true,
                IsCollapsed = true,
                CanBeRemovedByUser = false,
                Position = new Point(10, 20),
                Inputs = { new NodeInputViewModel() },
                Outputs = { new NodeOutputViewModel() }
            };

            NetworkViewModel network = new NetworkViewModel()
            {
                Nodes = { node }
            };

            NodeViewModel clone = Clone(node, framework);

            AssertNodesEqual(node, clone);

            node.IsCollapsed = false;
            clone = Clone(node, framework);
            network.Nodes.Add(clone);
            Assert.AreEqual(network, clone.Parent);
            Assert.AreEqual(1, clone.VisibleInputs.Count);
            Assert.AreEqual(1, clone.VisibleOutputs.Count);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestPendingConnection(SerializationFramework framework)
        {
            NetworkViewModel network = new NetworkViewModel();
            PendingConnectionViewModel vm = new PendingConnectionViewModel(network)
            {
                Input = new NodeInputViewModel(),
                Output = null,
                InputIsLocked = true,
                OutputIsLocked = false,
                LooseEndPoint = new Point(5,6),
                Validation = null
            };

            var clone = Clone(vm, framework);
            
            AssertPendingConnectionsEqual(vm, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestPort(SerializationFramework framework)
        {
            PortViewModel port = new PortViewModel
            {
                CenterPoint = new Point(50, 50),
                IsMirrored = true,
                IsVisible = false,
                IsHighlighted = true,
                IsInErrorMode = true
            };

            Endpoint parent = new NodeInputViewModel
            {
                Port = port
            };

            PortViewModel clone = Clone(port, framework);

            AssertPortsEqual(port, clone);
        }

        [DataTestMethod]
        [DataRow(SerializationFramework.JsonDotNet)]
        public void TestSelectionRectangle(SerializationFramework framework)
        {
            SelectionRectangleViewModel vm = new SelectionRectangleViewModel
            {
                StartPoint = new Point(1,2),
                EndPoint = new Point(3,4),
                IsVisible = true,
                IntersectingNodes = { new NodeViewModel() }
            };

            var clone = Clone(vm, framework);

            AssertSelectionRectanglesEqual(vm, clone);
        }

        private void AssertSequenceEqual<T>(IList<T> a, IList<T> b, Action<T, T> assertFunc)
        {
            Assert.AreEqual(a.Count, b.Count);
            foreach (var t in Enumerable.Zip(a, b, (x, y) => (x, y)))
                assertFunc(t.Item1, t.Item2);
        }

        private void AssertNetworksEqual(NetworkViewModel a, NetworkViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            AssertSequenceEqual(a.Nodes, b.Nodes, AssertNodesEqual);
            Assert.IsTrue(a.Nodes.All(n => n.Parent == a));
            Assert.IsTrue(b.Nodes.All(n => n.Parent == b));

            AssertSequenceEqual(a.SelectedNodes.ToList(), b.SelectedNodes.ToList(), AssertNodesEqual);
            Assert.IsTrue(a.SelectedNodes.All(n => n.Parent == a));
            Assert.IsTrue(b.SelectedNodes.All(n => n.Parent == b));

            AssertSequenceEqual(a.Connections, b.Connections, AssertConnectionsEqual);
            Assert.IsTrue(a.Connections.All(n => n.Parent == a));
            Assert.IsTrue(b.Connections.All(n => n.Parent == b));

            AssertNodesEqual(a.PendingNode, b.PendingNode);
            Assert.AreEqual(a.IsReadOnly, b.IsReadOnly);
            AssertCutLineEqual(a.CutLine, b.CutLine);
            AssertSelectionRectanglesEqual(a.SelectionRectangle, b.SelectionRectangle);

            AssertPendingConnectionsEqual(a.PendingConnection, b.PendingConnection);
            Assert.IsTrue(a.PendingConnection.Parent == a);
            Assert.IsTrue(b.PendingConnection.Parent == b);

            AssertNetworkValidationsEqual(a.LatestValidation, b.LatestValidation);
        }

        private void AssertNodesEqual(NodeViewModel a, NodeViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.CanBeRemovedByUser, b.CanBeRemovedByUser);
            Assert.AreEqual(a.IsCollapsed, b.IsCollapsed);
            Assert.AreEqual(a.IsSelected, b.IsSelected);
            Assert.AreEqual(a.Position, b.Position);
            
            AssertSequenceEqual(a.Inputs.ToList(), b.Inputs.ToList(), AssertInputsEqual);
            Assert.IsTrue(a.Inputs.All(n => n.Parent == a));
            Assert.IsTrue(b.Inputs.All(n => n.Parent == b));
            AssertSequenceEqual(a.Outputs.ToList(), b.Outputs.ToList(), AssertOutputsEqual);
            Assert.IsTrue(a.Outputs.All(n => n.Parent == a));
            Assert.IsTrue(b.Outputs.All(n => n.Parent == b));

            AssertSequenceEqual(a.VisibleInputs.ToList(), b.VisibleInputs.ToList(), AssertInputsEqual);
            AssertSequenceEqual(a.VisibleOutputs.ToList(), b.VisibleOutputs.ToList(), AssertOutputsEqual);
        }

        private void AssertEndpointsEqual(Endpoint a, Endpoint b)
        {
            if (a == null && b == null)
            {
                return;
            }

            if (a is NodeInputViewModel i1 && b is NodeInputViewModel i2)
            {
                AssertInputsEqual(i1, i2);
            }
            else if (a is NodeOutputViewModel o1 && b is NodeOutputViewModel o2)
            {
                AssertOutputsEqual(o1, o2);
            }
            else
            {
                Assert.Fail();
            }
        }

        private void AssertInputsEqual(NodeInputViewModel a, NodeInputViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.HideEditorIfConnected, b.HideEditorIfConnected);
            //Assert.AreEqual(a.Connections.Count, b.Connections.Count);
            Assert.AreEqual(a.MaxConnections, b.MaxConnections);
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.PortPosition, b.PortPosition);
            Assert.AreEqual(a.IsEditorVisible, b.IsEditorVisible);
            AssertEditorsEqual(a.Editor, b.Editor);
            if (a.Editor != null)
            {
                Assert.IsTrue(a.Editor.Parent == a);
                Assert.IsTrue(b.Editor.Parent == b);
            }
            AssertPortsEqual(a.Port, b.Port);
            Assert.IsTrue(a.Port.Parent == a);
            Assert.IsTrue(b.Port.Parent == b);
        }

        private void AssertOutputsEqual(NodeOutputViewModel a, NodeOutputViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            //Assert.AreEqual(a.Connections.Count, b.Connections.Count);
            Assert.AreEqual(a.MaxConnections, b.MaxConnections);
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.PortPosition, b.PortPosition);
            AssertEditorsEqual(a.Editor, b.Editor);
            if (a.Editor != null)
            {
                Assert.IsTrue(a.Editor.Parent == a);
                Assert.IsTrue(b.Editor.Parent == b);
            }
            AssertPortsEqual(a.Port, b.Port);
            Assert.IsTrue(a.Port.Parent == a);
            Assert.IsTrue(b.Port.Parent == b);
        }
        
        private void AssertEditorsEqual(NodeEndpointEditorViewModel a, NodeEndpointEditorViewModel b)
        {
        }

        private void AssertPortsEqual(PortViewModel a, PortViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.IsHighlighted, b.IsHighlighted);
            Assert.AreEqual(a.IsVisible, b.IsVisible);
            Assert.AreEqual(a.IsInErrorMode, b.IsInErrorMode);
            Assert.AreEqual(a.IsMirrored, b.IsMirrored);
            Assert.AreEqual(a.CenterPoint, b.CenterPoint);
        }

        private void AssertConnectionsEqual(ConnectionViewModel a, ConnectionViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.CanBeRemovedByUser, b.CanBeRemovedByUser);
            Assert.AreEqual(a.IsHighlighted, b.IsHighlighted);
            Assert.AreEqual(a.IsInErrorState, b.IsInErrorState);
            Assert.AreEqual(a.IsMarkedForDelete, b.IsMarkedForDelete);
            AssertInputsEqual(a.Input, b.Input);
            AssertOutputsEqual(a.Output, b.Output);
        }

        private void AssertPendingConnectionsEqual(PendingConnectionViewModel a, PendingConnectionViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.InputIsLocked, b.InputIsLocked);
            Assert.AreEqual(a.OutputIsLocked, b.OutputIsLocked);
            Assert.AreEqual(a.BoundingBox, b.BoundingBox);
            Assert.AreEqual(a.LooseEndPoint, b.LooseEndPoint);
            AssertInputsEqual(a.Input, b.Input);
            AssertOutputsEqual(a.Output, b.Output);
            AssertConnectionValidationsEqual(a.Validation, b.Validation);
        }

        private void AssertErrorMessagesEqual(ErrorMessageViewModel a, ErrorMessageViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.Message, b.Message);
        }

        private void AssertCutLineEqual(CutLineViewModel a, CutLineViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.IsVisible, b.IsVisible);
            Assert.AreEqual(a.EndPoint, b.EndPoint);
            Assert.AreEqual(a.StartPoint, b.StartPoint);

            //AssertSequenceEqual(a.IntersectingConnections, b.IntersectingConnections, AssertConnectionsEqual);
        }

        private void AssertSelectionRectanglesEqual(SelectionRectangleViewModel a, SelectionRectangleViewModel b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.IsVisible, b.IsVisible);
            Assert.AreEqual(a.StartPoint, b.StartPoint);
            Assert.AreEqual(a.EndPoint, b.EndPoint);
            Assert.AreEqual(a.Rectangle, b.Rectangle);

            //AssertSequenceEqual(a.IntersectingNodes, b.IntersectingNodes, AssertNodesEqual);
        }

        private void AssertNetworkValidationsEqual(NetworkValidationResult a, NetworkValidationResult b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.IsValid, b.IsValid);
            Assert.AreEqual(a.NetworkIsTraversable, b.NetworkIsTraversable);
        }

        private void AssertConnectionValidationsEqual(ConnectionValidationResult a, ConnectionValidationResult b)
        {
            if (a == null && b == null)
            {
                return;
            }

            Assert.AreEqual(a.IsValid, b.IsValid);
        }

        public enum SerializationFramework
        {
            JsonDotNet
        }

        private T Clone<T>(T obj, SerializationFramework framework)
        {
            switch (framework)
            {
                case SerializationFramework.JsonDotNet:
                    var settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        TypeNameHandling = TypeNameHandling.All
                    };
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, settings), settings);
            }
            throw new Exception("Unsupported framework");
        }
    }
}
