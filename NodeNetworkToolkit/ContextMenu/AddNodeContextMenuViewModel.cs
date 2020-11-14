using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Windows;
using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ContextMenu
{
    /// <summary>
    /// A viewmodel for a context menu that allows users to add nodes to a network.
    /// </summary>
    public class AddNodeContextMenuViewModel : SearchableContextMenuViewModel
    {
        static AddNodeContextMenuViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new SearchableContextMenuView(), typeof(IViewFor<AddNodeContextMenuViewModel>));
        }

        #region Network
        /// <summary>
        /// The network to which the nodes are to be added.
        /// </summary>
        public NetworkViewModel Network
        {
            get => _network;
            set => this.RaiseAndSetIfChanged(ref _network, value);
        }
        private NetworkViewModel _network;
        #endregion

        /// <summary>
        /// The format that is used to create labels for the menu entries based on the node name.
        /// E.g. "Add {0}"
        /// </summary>
        public string LabelFormat { get; }

        /// <summary>
        /// When adding a node to the network,
        /// this function is used to determine the position at which it is placed.
        /// </summary>
        public Func<NodeViewModel, Point> NodePositionFunc { get; set; } = (node) => new Point();

        /// <summary>
        /// A callback that is called after a node is added to the network through this menu.
        /// </summary>
        public Action<NodeViewModel> OnNodeAdded { get; set; } = node => { };

        /// <summary>
        /// An interaction that is used to open contextmenu views given a SearchableContextMenuViewModel.
        /// Used in ShowAddNodeForPendingConnectionMenu to display this menu, and a menu for choosing an endpoint.
        /// </summary>
        public Interaction<SearchableContextMenuViewModel, Unit> OpenContextMenu { get; } = new Interaction<SearchableContextMenuViewModel, Unit>();

        private ReactiveCommand<NodeTemplate, Unit> CreateNode { get; }

        public AddNodeContextMenuViewModel(string labelFormat = "{0}")
        {
            LabelFormat = labelFormat;

            CreateNode = ReactiveCommand.Create<NodeTemplate, Unit>((template) =>
            {
                var nodeInstance = template.Factory();
                Network.Nodes.Add(nodeInstance);
                nodeInstance.Position = NodePositionFunc(nodeInstance);
                OnNodeAdded(nodeInstance);
                return Unit.Default;
            });
        }

        /// <summary>
        /// Adds a new node type to the list.
        /// Every time a node is added to a network from this list, the factory function in the template
        /// will be called to create a new instance of the viewmodel type.
        /// </summary>
        /// <param name="template">The template with the node type to add.</param>
        public void AddNodeType(NodeTemplate template)
        {
            Commands.Add(new LabeledCommand
            {
                Label = string.Format(LabelFormat, template.Instance.Name),
                Command = CreateNode,
                CommandParameter = template
            });
        }

        public void AddNodeTypes(IEnumerable<NodeTemplate> templates)
        {
            foreach (var nodeTemplate in templates)
            {
                AddNodeType(nodeTemplate);
            }
        }

        private void ShowOnlyConnectableNodes(PendingConnectionViewModel testCon)
        {
            foreach (var cmd in Commands.Items)
            {
                var curNodeTemplate = (NodeTemplate)cmd.CommandParameter;

                bool hasValidEndpoint =
                    testCon.InputIsLocked ?
                        GetConnectableOutputs(curNodeTemplate.Instance, testCon).Any() :
                        GetConnectableInputs(curNodeTemplate.Instance, testCon).Any();

                cmd.Visible = hasValidEndpoint;
            }
        }

        public void ShowAddNodeForPendingConnectionMenu(PendingConnectionViewModel pendingCon)
        {
            var testCon = new PendingConnectionViewModel(pendingCon.Parent) // Copy used to test which inputs/outputs will work with the pending connection
            {
                Input = pendingCon.Input,
                InputIsLocked = pendingCon.InputIsLocked,
                Output = pendingCon.Output,
                OutputIsLocked = pendingCon.OutputIsLocked
            };

            ShowOnlyConnectableNodes(testCon);

            // After a node type is chosen, pick an endpoint
            OnNodeAdded = node =>
            {
                if (testCon.InputIsLocked)
                {
                    var outputs = GetConnectableOutputs(node, testCon).ToList();
                    if (outputs.Count == 1)
                    {
                        // If only 1 output matches, select this one
                        Network.Connections.Add(Network.ConnectionFactory(pendingCon.Input, outputs[0]));
                        Network.RemovePendingConnection();
                    }
                    else
                    {
                        // Open a menu to let the user choose the desired output to connect to
                        var chooseEndpointVM = new SearchableContextMenuViewModel();
                        var cmd = ReactiveCommand.Create<NodeOutputViewModel, Unit>((o) =>
                        {
                            Network.Connections.Add(Network.ConnectionFactory(pendingCon.Input, o));
                            Network.RemovePendingConnection();
                            return Unit.Default;
                        });
                        foreach (var output in outputs)
                        {
                            chooseEndpointVM.Commands.Add(new LabeledCommand
                            {
                                Command = cmd,
                                CommandParameter = output,
                                Label = output.Name
                            });
                        }

                        OpenContextMenu.Handle(chooseEndpointVM).Subscribe();
                    }
                }
                else
                {
                    var inputs = GetConnectableInputs(node, testCon).ToList();
                    if (inputs.Count == 1)
                    {
                        Network.Connections.Add(Network.ConnectionFactory(inputs[0], pendingCon.Output));
                        Network.RemovePendingConnection();
                    }
                    else
                    {
                        var chooseEndpointVM = new SearchableContextMenuViewModel();
                        var cmd = ReactiveCommand.Create<NodeInputViewModel, Unit>((i) =>
                        {
                            Network.Connections.Add(Network.ConnectionFactory(i, pendingCon.Output));
                            Network.RemovePendingConnection();
                            return Unit.Default;
                        });
                        foreach (var input in inputs)
                        {
                            chooseEndpointVM.Commands.Add(new LabeledCommand
                            {
                                Command = cmd,
                                CommandParameter = input,
                                Label = input.Name
                            });
                        }

                        OpenContextMenu.Handle(chooseEndpointVM).Subscribe();
                    }
                }
            };

            OpenContextMenu.Handle(this).Subscribe();
        }

        /// <summary>
        /// Given a set of node templates, return those which have an endpoint
        /// that could be connected to the specified pending connection.
        /// </summary>
        public static IEnumerable<NodeTemplate> GetConnectableNodes(IEnumerable<NodeTemplate> candidateNodeTemplates, PendingConnectionViewModel testCon)
        {
            foreach (var curNode in candidateNodeTemplates)
            {
                bool hasValidEndpoint =
                    testCon.InputIsLocked ?
                        GetConnectableOutputs(curNode.Instance, testCon).Any() :
                        GetConnectableInputs(curNode.Instance, testCon).Any();

                if (hasValidEndpoint)
                {
                    yield return curNode;
                }
            }
        }

        /// <summary>
        /// Given a node viewmodel, return the outputs which could be connected to the pending connection.
        /// Assumes testCon.Input is set.
        /// </summary>
        public static IEnumerable<NodeOutputViewModel> GetConnectableOutputs(NodeViewModel node, PendingConnectionViewModel testCon)
        {
            var validator = testCon.Input.ConnectionValidator;
            foreach (var curOutput in node.Outputs.Items)
            {
                testCon.Output = curOutput;
                if (curOutput.MaxConnections > 0 && validator(testCon).IsValid)
                {
                    yield return curOutput;
                }
            }
        }

        /// <summary>
        /// Given a node viewmodel, return the inputs which could be connected to the pending connection.
        /// Assumes testCon.Output is set.
        /// </summary>
        public static IEnumerable<NodeInputViewModel> GetConnectableInputs(NodeViewModel node, PendingConnectionViewModel testCon)
        {
            foreach (var curInput in node.Inputs.Items)
            {
                var validator = curInput.ConnectionValidator;
                testCon.Input = curInput;
                if (curInput.MaxConnections > 0 && validator(testCon).IsValid)
                {
                    yield return curInput;
                }
            }
        }
    }
}
