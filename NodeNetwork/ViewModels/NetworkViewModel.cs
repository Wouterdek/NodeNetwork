using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// The viewmodel for node networks.
    /// </summary>
    public class NetworkViewModel : ReactiveObject
    {
        static NetworkViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NetworkView(), typeof(IViewFor<NetworkViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Nodes
        /// <summary>
        /// The list of nodes in this network.
        /// </summary>
        public IReactiveList<NodeViewModel> Nodes => _nodes;
        private readonly ReactiveList<NodeViewModel> _nodes = new ReactiveList<NodeViewModel> { ChangeTrackingEnabled = true };
        #endregion

        #region SelectedNodes
        /// <summary>
        /// A list of nodes that are currently selected in the UI.
        /// The contents of this list is equal to the nodes in Nodes where the Selected property is true.
        /// </summary>
        public IReactiveDerivedList<NodeViewModel> SelectedNodes { get; }
        #endregion

        #region Connections
        /// <summary>
        /// The list of connections in this network.
        /// </summary>
        public IReactiveList<ConnectionViewModel> Connections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion

        #region PendingConnection 
        /// <summary>
        /// The connection that is currently being build by the user.
        /// This connection is visually displayed in the UI, but is not an actual functional connection.
        /// This is used when the user drags from an endpoint to create a new connection.
        /// </summary>
        public PendingConnectionViewModel PendingConnection
        {
            get => _pendingConnection;
            set => this.RaiseAndSetIfChanged(ref _pendingConnection, value);
        }
        private PendingConnectionViewModel _pendingConnection;
        #endregion

        #region PendingNode
        /// <summary>
        /// The viewmodel of the node that is not part of the network, but is displayed as a node that can be added.
        /// This property is used to display a new node when the user drags a node viewmodel over the network view.
        /// </summary>
        public NodeViewModel PendingNode
        {
            get => _pendingNode;
            set => this.RaiseAndSetIfChanged(ref _pendingNode, value);
        }
        private NodeViewModel _pendingNode;
        #endregion

        #region ConnectionFactory
        /// <summary>
        /// The function that is used to create connection viewmodels when the user creates connections in the network view.
        /// By default, this function creates a ConnectionViewModel.
        /// </summary>
        public Func<NodeInputViewModel, NodeOutputViewModel, ConnectionViewModel> ConnectionFactory
        {
            get => _connectionFactory;
            set => this.RaiseAndSetIfChanged(ref _connectionFactory, value);
        }
        private Func<NodeInputViewModel, NodeOutputViewModel, ConnectionViewModel> _connectionFactory;
        #endregion

        #region Validator
        /// <summary>
        /// Function that is used to check if the network is valid or not.
        /// To run the validation, use the UpdateValidation command.
        /// </summary>
        public Func<NetworkViewModel, NetworkValidationResult> Validator
        {
            get => _validator;
            set => this.RaiseAndSetIfChanged(ref _validator, value);
        }
        private Func<NetworkViewModel, NetworkValidationResult> _validator;
        #endregion

        #region LatestValidation
        //Using ObservableAsPropertyHelper would be better, but causes problems with ReactiveCommand where
        //the value of the property is updated only after the subscribers to the command are run.

        /// <summary>
        /// The validation of the current state of the network.
        /// This property is automatically updated when UpdateValidation runs.
        /// </summary>
        public NetworkValidationResult LatestValidation
        {
            get => _latestValidation;
            private set => this.RaiseAndSetIfChanged(ref _latestValidation, value);
        }
        private NetworkValidationResult _latestValidation;
        #endregion

        #region Validation
        /// <summary>
        /// Observable that produces the latest NetworkValidationResult every time the network is validated.
        /// </summary>
        public IObservable<NetworkValidationResult> Validation { get; }
        #endregion

        #region IsReadOnly
        /// <summary>
        /// If true, the network and its contents (nodes, connections, input/output editors, ...) cannot be modified by the user.
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }
        private bool _isReadOnly;
        #endregion

        #region CutLine
        /// <summary>
        /// The viewmodel of the cutline used in this network view.
        /// </summary>
        public CutLineViewModel CutLine { get; } = new CutLineViewModel();
        #endregion
        
        #region SelectionRectangle
        /// <summary>
        /// The viewmodel for the selection rectangle used in this network view.
        /// </summary>
        public SelectionRectangleViewModel SelectionRectangle { get; } = new SelectionRectangleViewModel();
        #endregion

        #region Commands
        /// <summary>
        /// Deletes the nodes in SelectedNodes that are user-removable.
        /// </summary>
        public ReactiveCommand DeleteSelectedNodes { get; }

        /// <summary>
        /// Runs the Validator function and stores the result in LatestValidation.
        /// </summary>
        public ReactiveCommand<Unit, NetworkValidationResult> UpdateValidation { get; }
        #endregion

        public NetworkViewModel()
        {
            // Setup parent relationship in nodes.
            Nodes.ActOnEveryObject(
                addedNode => addedNode.Parent = this,
                removedNode => removedNode.Parent = null
            );
            
            // SelectedNodes is a derived collection of all nodes with IsSelected = true.
            SelectedNodes = Nodes.CreateDerivedCollection(node => node, node => node.IsSelected);

            // When DeleteSelectedNodes is invoked, remove all nodes that are user-removable and selected.
            DeleteSelectedNodes = ReactiveCommand.Create(() =>
            {
                Nodes.RemoveAll(SelectedNodes.Where(n => n.CanBeRemovedByUser).ToArray());
            });

            // When a node is removed, delete any connections from/to that node.
            Nodes.ItemsRemoved.Subscribe(removedNode =>
            {
                Connections.RemoveAll(removedNode.Inputs.SelectMany(o => o.Connections).ToArray());
                Connections.RemoveAll(removedNode.Outputs.SelectMany(o => o.Connections).ToArray());

                bool pendingConnectionInvalid = PendingConnection?.Input?.Parent == removedNode ||
                                                PendingConnection?.Output?.Parent == removedNode;
                if (pendingConnectionInvalid)
                {
                    RemovePendingConnection();
                }
            });
            
            // When the list of nodes is reset, remove any connections whose input/output node was removed.
            Nodes.ShouldReset.Subscribe(_ =>
            {
                // Create a hashset with all nodes for O(1) search
                HashSet<NodeViewModel> nodeSet = new HashSet<NodeViewModel>(Nodes);

                for (var i = Connections.Count - 1; i >= 0; i--)
                {
                    var conn = Connections[i];

                    if (!nodeSet.Contains(conn.Input.Parent) || !nodeSet.Contains(conn.Output.Parent))
                    {
                        Connections.RemoveAt(i);
                    }
                }

                var pendingConnInputNode = PendingConnection?.Input?.Parent;
                var pendingConnOutputNode = PendingConnection?.Output?.Parent;
                bool pendingConnectionInvalid = (pendingConnInputNode != null && !nodeSet.Contains(pendingConnInputNode)) ||
                                                (pendingConnOutputNode != null && !nodeSet.Contains(pendingConnOutputNode));
                if (pendingConnectionInvalid)
                {
                    RemovePendingConnection();
                }
            });

            // Setup a default ConnectionFactory that will be used to create connections.
            ConnectionFactory = (input, output) => new ConnectionViewModel(this, input, output);

            // Setup a default network validator that always returns valid.
            Validator = _ => new NetworkValidationResult(true, true, null);

            // Setup the validation command.
            UpdateValidation = ReactiveCommand.Create(() => {
                var result = Validator(this);
                LatestValidation = result;
                return result;
            });

            // Setup Validation observable
            var onValidationPropertyUpdate = this.WhenAnyValue(vm => vm.LatestValidation).Multicast(new Subject<NetworkValidationResult>());
            onValidationPropertyUpdate.Connect();
            Validation = Observable.Defer(() => Observable.Return(LatestValidation)).Concat(onValidationPropertyUpdate);
            
            // When a connection or node changes, validate the network.
            Connections.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation);
            Nodes.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation);
        }
        
        /// <summary>
        /// Clears SelectedNodes, setting the IsSelected property of all the nodes to false.
        /// </summary>
        public void ClearSelection()
        {
            using (SelectedNodes.SuppressChangeNotifications())
            {
                foreach (NodeViewModel node in SelectedNodes.ToArray())
                {
                    node.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Starts a cut in the CutLine viewmodel.
        /// </summary>
        public void StartCut()
        {
            CutLine.IsVisible = true;
        }

        /// <summary>
        /// Stops the current cut in the CutLine viewmodel and applies the changes.
        /// </summary>
        public void FinishCut()
        {
            Connections.RemoveAll(CutLine.IntersectingConnections);
            CutLine.IntersectingConnections.Clear();
            CutLine.IsVisible = false;
        }

        /// <summary>
        /// Sets PendingConnection to null.
        /// </summary>
        public void RemovePendingConnection()
        {
            PendingConnection = null;
        }

        /// <summary>
        /// Starts a selection in RectangleSelection
        /// </summary>
        public void StartRectangleSelection()
        {
            ClearSelection();
            SelectionRectangle.IsVisible = true;
            SelectionRectangle.IntersectingNodes.Clear();
        }

        /// <summary>
        /// Stops the current selection in RectangleSelection and applies the changes.
        /// </summary>
        public void FinishRectangleSelection()
        {
            SelectionRectangle.IsVisible = false;
        }
    }
}
