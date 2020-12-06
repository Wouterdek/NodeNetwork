using System.Collections.Generic;
using System.Linq;

using DynamicData;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NodeNetwork.ViewModels;

namespace NodeNetworkTests
{
    [TestClass]
    public class EndpointGroupingTests
    {
        [TestMethod]
        public void TestNewNodeHasNoGroups()
        {
            NodeViewModel node = new NodeViewModel();

            Assert.IsFalse(node.VisibleEndpointGroups.Any());
        }

        [TestMethod]
        public void TestAddingUngroupedEndpoints()
        {
            NodeViewModel node = new NodeViewModel();

            node.Inputs.Add(new NodeInputViewModel());
            node.Outputs.Add(new NodeOutputViewModel());

            Assert.IsFalse(node.VisibleEndpointGroups.Any());
        }

        [TestMethod]
        public void TestGroupedEndpoints()
        {
            NodeViewModel node = new NodeViewModel();

            EndpointGroup groupA = new EndpointGroup();
            EndpointGroup groupB = new EndpointGroup();
            
            NodeInputViewModel inputA1 = new NodeInputViewModel { Group = groupA };
            NodeInputViewModel inputA2 = new NodeInputViewModel { Group = groupA };
            NodeOutputViewModel outputA1 = new NodeOutputViewModel { Group = groupA };
            NodeOutputViewModel outputA2 = new NodeOutputViewModel { Group = groupA };

            NodeInputViewModel inputB1 = new NodeInputViewModel { Group = groupB };
            NodeInputViewModel inputB2 = new NodeInputViewModel { Group = groupB };
            NodeOutputViewModel outputB1 = new NodeOutputViewModel { Group = groupB };
            NodeOutputViewModel outputB2 = new NodeOutputViewModel { Group = groupB };

            node.Inputs.Add(inputB1);

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 1);
            EndpointGroupViewModel groupBViewModel = node.VisibleEndpointGroups[0];
            Assert.IsTrue(groupBViewModel.Group == groupB);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputB1, groupBViewModel.VisibleInputs.Items.First());

            node.Outputs.Add(outputB2);

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 1);
            groupBViewModel = node.VisibleEndpointGroups[0];
            Assert.IsTrue(groupBViewModel.Group == groupB);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputB1, groupBViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupBViewModel.VisibleOutputs.Count == 1);
            Assert.AreEqual(outputB2, groupBViewModel.VisibleOutputs.Items.First());

            node.Inputs.AddRange(new []{ inputA1, inputB2, inputA2 });
            node.Outputs.AddRange(new []{ outputB1, outputA1, outputA2 });

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 2);
            groupBViewModel = node.VisibleEndpointGroups[0];
            Assert.IsTrue(groupBViewModel.Group == groupB);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 2);
            Assert.AreEqual(inputB1, groupBViewModel.VisibleInputs.Items.First());
            Assert.AreEqual(inputB2, groupBViewModel.VisibleInputs.Items.ElementAt(1));
            Assert.IsTrue(groupBViewModel.VisibleOutputs.Count == 2);
            Assert.AreEqual(outputB2, groupBViewModel.VisibleOutputs.Items.First());
            Assert.AreEqual(outputB1, groupBViewModel.VisibleOutputs.Items.ElementAt(1));

            EndpointGroupViewModel groupAViewModel = node.VisibleEndpointGroups[1];
            Assert.IsTrue(groupAViewModel.Group == groupA);
            Assert.IsTrue(groupAViewModel.VisibleInputs.Count == 2);
            Assert.AreEqual(inputA1, groupAViewModel.VisibleInputs.Items.First());
            Assert.AreEqual(inputA2, groupAViewModel.VisibleInputs.Items.ElementAt(1));
            Assert.IsTrue(groupAViewModel.VisibleOutputs.Count == 2);
            Assert.AreEqual(outputA1, groupAViewModel.VisibleOutputs.Items.First());
            Assert.AreEqual(outputA2, groupAViewModel.VisibleOutputs.Items.ElementAt(1));

            node.Inputs.Remove(inputB1);

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 2);
            groupBViewModel = node.VisibleEndpointGroups[0];
            Assert.IsTrue(groupBViewModel.Group == groupB);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputB2, groupBViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupBViewModel.VisibleOutputs.Count == 2);
            Assert.AreEqual(outputB2, groupBViewModel.VisibleOutputs.Items.First());
            Assert.AreEqual(outputB1, groupBViewModel.VisibleOutputs.Items.ElementAt(1));

            groupAViewModel = node.VisibleEndpointGroups[1];
            Assert.IsTrue(groupAViewModel.Group == groupA);
            Assert.IsTrue(groupAViewModel.VisibleInputs.Count == 2);
            Assert.AreEqual(inputA1, groupAViewModel.VisibleInputs.Items.First());
            Assert.AreEqual(inputA2, groupAViewModel.VisibleInputs.Items.ElementAt(1));
            Assert.IsTrue(groupAViewModel.VisibleOutputs.Count == 2);
            Assert.AreEqual(outputA1, groupAViewModel.VisibleOutputs.Items.First());
            Assert.AreEqual(outputA2, groupAViewModel.VisibleOutputs.Items.ElementAt(1));

            node.Inputs.Remove(inputB2);
            node.Outputs.RemoveMany(new []{outputB1, outputB2});

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 1);

            groupAViewModel = node.VisibleEndpointGroups[0];
            Assert.IsTrue(groupAViewModel.Group == groupA);
            Assert.IsTrue(groupAViewModel.VisibleInputs.Count == 2);
            Assert.AreEqual(inputA1, groupAViewModel.VisibleInputs.Items.First());
            Assert.AreEqual(inputA2, groupAViewModel.VisibleInputs.Items.ElementAt(1));
            Assert.IsTrue(groupAViewModel.VisibleOutputs.Count == 2);
            Assert.AreEqual(outputA1, groupAViewModel.VisibleOutputs.Items.First());
            Assert.AreEqual(outputA2, groupAViewModel.VisibleOutputs.Items.ElementAt(1));
        }

        [TestMethod]
        public void TestNestedGroups()
        {
            NodeViewModel node = new NodeViewModel();

            EndpointGroup groupA = new EndpointGroup { Name = "Group A" };
            EndpointGroup groupB = new EndpointGroup { Name = "Group B" };
            EndpointGroup groupC = new EndpointGroup(groupA) { Name = "Group C" };
            EndpointGroup groupD = new EndpointGroup(groupB) { Name = "Group D" };
            
            NodeInputViewModel inputC = new NodeInputViewModel { Group = groupC, Name = "Input C"};
            NodeOutputViewModel outputC = new NodeOutputViewModel { Group = groupC, Name = "Output C" };

            NodeInputViewModel inputD = new NodeInputViewModel { Group = groupD, Name = "Input D" };
            NodeOutputViewModel outputD = new NodeOutputViewModel { Group = groupD, Name = "Output D" };

            node.Inputs.Add(inputC);
            node.Inputs.Add(inputD);
            node.Outputs.Add(outputC);
            node.Outputs.Add(outputD);

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleOutputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 2);

            EndpointGroupViewModel groupAViewModel = node.VisibleEndpointGroups[0];
            Assert.AreEqual(groupA, groupAViewModel.Group);
            Assert.IsTrue(groupAViewModel.VisibleInputs.Count == 0);
            Assert.IsTrue(groupAViewModel.VisibleOutputs.Count == 0);
            Assert.IsTrue(groupAViewModel.Children.Count == 1);
            EndpointGroupViewModel groupCViewModel = groupAViewModel.Children[0];
            Assert.AreEqual(groupC, groupCViewModel.Group);
            Assert.IsTrue(groupCViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputC, groupCViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupCViewModel.VisibleOutputs.Count == 1);
            Assert.AreEqual(outputC, groupCViewModel.VisibleOutputs.Items.First());

            EndpointGroupViewModel groupBViewModel = node.VisibleEndpointGroups[1];
            Assert.AreEqual(groupB, groupBViewModel.Group);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 0);
            Assert.IsTrue(groupBViewModel.VisibleOutputs.Count == 0);
            Assert.IsTrue(groupBViewModel.Children.Count == 1);
            EndpointGroupViewModel groupDViewModel = groupBViewModel.Children[0];
            Assert.AreEqual(groupD, groupDViewModel.Group);
            Assert.IsTrue(groupDViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputD, groupDViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupDViewModel.VisibleOutputs.Count == 1);
            Assert.AreEqual(outputD, groupDViewModel.VisibleOutputs.Items.First());

            node.Inputs.Remove(inputC);
            node.Outputs.Remove(outputC);

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleOutputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 1);

            groupBViewModel = node.VisibleEndpointGroups[0];
            Assert.AreEqual(groupB, groupBViewModel.Group);
            Assert.IsTrue(groupBViewModel.VisibleInputs.Count == 0);
            Assert.IsTrue(groupBViewModel.VisibleOutputs.Count == 0);
            Assert.IsTrue(groupBViewModel.Children.Count == 1);
            groupDViewModel = groupBViewModel.Children[0];
            Assert.AreEqual(groupD, groupDViewModel.Group);
            Assert.IsTrue(groupDViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputD, groupDViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupDViewModel.VisibleOutputs.Count == 1);
            Assert.AreEqual(outputD, groupDViewModel.VisibleOutputs.Items.First());
        }

        [TestMethod]
        public void TestCollapseWithGroups()
        {
            NodeViewModel node = new NodeViewModel();

            EndpointGroup groupA = new EndpointGroup { Name = "Group A" };
            EndpointGroup groupB = new EndpointGroup { Name = "Group B" };
            EndpointGroup groupC = new EndpointGroup(groupA) { Name = "Group C" };
            EndpointGroup groupD = new EndpointGroup(groupB) { Name = "Group D" };
            
            NodeInputViewModel inputC = new NodeInputViewModel { Group = groupC, Name = "Input C"};
            NodeOutputViewModel outputC = new NodeOutputViewModel { Group = groupC, Name = "Output C" };

            NodeInputViewModel inputD = new NodeInputViewModel { Group = groupD, Name = "Input D" };
            NodeOutputViewModel outputD = new NodeOutputViewModel { Group = groupD, Name = "Output D" };

            node.Inputs.Add(inputC);
            node.Inputs.Add(inputD);
            node.Outputs.Add(outputC);
            node.Outputs.Add(outputD);

            var network = new NetworkViewModel();
            network.Nodes.Add(node);
            network.Connections.Add(network.ConnectionFactory(inputC, new NodeOutputViewModel()));

            node.IsCollapsed = true;

            Assert.IsTrue(node.VisibleInputs.Count == 0);
            Assert.IsTrue(node.VisibleOutputs.Count == 0);
            Assert.IsTrue(node.VisibleEndpointGroups.Count == 1);

            EndpointGroupViewModel groupAViewModel = node.VisibleEndpointGroups[0];
            Assert.AreEqual(groupA, groupAViewModel.Group);
            Assert.IsTrue(groupAViewModel.VisibleInputs.Count == 0);
            Assert.IsTrue(groupAViewModel.VisibleOutputs.Count == 0);
            Assert.IsTrue(groupAViewModel.Children.Count == 1);
            EndpointGroupViewModel groupCViewModel = groupAViewModel.Children[0];
            Assert.AreEqual(groupC, groupCViewModel.Group);
            Assert.IsTrue(groupCViewModel.VisibleInputs.Count == 1);
            Assert.AreEqual(inputC, groupCViewModel.VisibleInputs.Items.First());
            Assert.IsTrue(groupCViewModel.VisibleOutputs.Count == 0);
        }
    }
}