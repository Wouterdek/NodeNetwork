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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// The viewmodel for node networks.
    /// </summary>
    [DataContract]
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
        [DataMember]
        public IReactiveList<NodeViewModel> Nodes { get; } = new ReactiveList<NodeViewModel> { ChangeTrackingEnabled = true };
        #endregion

        #region SelectedNodes
        /// <summary>
        /// A list of nodes that are currently selected in the UI.
        /// The contents of this list is equal to the nodes in Nodes where the Selected property is true.
        /// </summary>
        [IgnoreDataMember]
        public IReactiveDerivedList<NodeViewModel> SelectedNodes { get; private set; }
        #endregion

        #region Connections
        /// <summary>
        /// The list of connections in this network.
        /// </summary>
        [DataMember]
        public IReactiveList<ConnectionViewModel> Connections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion

        #region PendingConnection 
        /// <summary>
        /// The connection that is currently being build by the user.
        /// This connection is visually displayed in the UI, but is not an actual functional connection.
        /// This is used when the user drags from an endpoint to create a new connection.
        /// </summary>
        [DataMember]
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
        [DataMember]
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
        /// </summary>
        [DataMember]
        public ConnectionFactory ConnectionFactory
        {
            get => _connectionFactory;
            set => this.RaiseAndSetIfChanged(ref _connectionFactory, value);
        }
        private ConnectionFactory _connectionFactory;
        #endregion

        #region Validator
        /// <summary>
        /// Function that is used to check if the network is valid or not.
        /// To run the validation, use the UpdateValidation command.
        /// </summary>
        [DataMember]
        public NetworkValidator Validator
        {
            get => _validator;
            set => this.RaiseAndSetIfChanged(ref _validator, value);
        }
        private NetworkValidator _validator;
        #endregion

        #region LatestValidation
        //Using ObservableAsPropertyHelper would be better, but causes problems with ReactiveCommand where
        //the value of the property is updated only after the subscribers to the command are run.

        /// <summary>
        /// The validation of the current state of the network.
        /// This property is automatically updated when UpdateValidation runs.
        /// </summary>
        [IgnoreDataMember]
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
        [IgnoreDataMember]
        public IObservable<NetworkValidationResult> Validation { get; private set; }
        #endregion

        #region IsReadOnly
        /// <summary>
        /// If true, the network and its contents (nodes, connections, input/output editors, ...) cannot be modified by the user.
        /// </summary>
        [DataMember]
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
        [DataMember]
        public CutLineViewModel CutLine { get; } = new CutLineViewModel();
        #endregion

        #region SelectionRectangle
        /// <summary>
        /// The viewmodel for the selection rectangle used in this network view.
        /// </summary>
        [DataMember]
        public SelectionRectangleViewModel SelectionRectangle { get; } = new SelectionRectangleViewModel();
        #endregion

        #region Commands
        /// <summary>
        /// Deletes the nodes in SelectedNodes that are user-removable.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand DeleteSelectedNodes { get; private set; }

        /// <summary>
        /// Runs the Validator function and stores the result in LatestValidation.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<Unit, NetworkValidationResult> UpdateValidation { get; private set; }
        #endregion

        public NetworkViewModel() => Setup();
        [OnDeserialized] internal void OnDeserialized(StreamingContext ctx) => Setup();
        private CompositeDisposable _setupDisposable;

        private void Setup()
        {
            //If already setup, clear bindings and redo setup
            _setupDisposable?.Dispose();
            _setupDisposable = new CompositeDisposable();
            
            Nodes.BeforeItemsAdded.Subscribe(node => node.Parent = this).DisposeWith(_setupDisposable);
            Nodes.BeforeItemsRemoved.Subscribe(node => node.Parent = null).DisposeWith(_setupDisposable);

            SelectedNodes = Nodes.CreateDerivedCollection(node => node, node => node.IsSelected).DisposeWith(_setupDisposable);

            DeleteSelectedNodes = ReactiveCommand.Create(() =>
            {
                var nodesToRemove = SelectedNodes.Where(n => n.CanBeRemovedByUser).ToArray();
                foreach (NodeViewModel node in nodesToRemove)
                {
                    Connections.RemoveAll(node.Inputs.SelectMany(o => o.Connections).ToArray());
                    Connections.RemoveAll(node.Outputs.SelectMany(o => o.Connections).ToArray());
                }

                bool pendingConnectionInvalid = new[] {PendingConnection?.Input?.Parent, PendingConnection?.Output?.Parent}.Any(n => nodesToRemove.Contains(n));
                if (pendingConnectionInvalid)
                {
                    RemovePendingConnection();
                }

                Nodes.RemoveAll(nodesToRemove);
            });

            ConnectionFactory = new ConnectionFactory();

            Validator = new NetworkValidator();
            UpdateValidation = ReactiveCommand.Create(() => {
                var result = Validator.Validate(this);
                LatestValidation = result;
                return result;
            });
            //UpdateValidation.ToProperty(this, vm => vm.LatestValidation, out _latestValidation);

            var onValidationPropertyUpdate = this.WhenAnyValue(vm => vm.LatestValidation).Multicast(new Subject<NetworkValidationResult>());
            onValidationPropertyUpdate.Connect().DisposeWith(_setupDisposable);
            Validation = Observable.Create<NetworkValidationResult>(obs =>
            {
                obs.OnNext(LatestValidation);
                obs.OnCompleted();
                return Disposable.Empty;
            }).Concat(onValidationPropertyUpdate);
            
            Connections.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation).DisposeWith(_setupDisposable);
            Nodes.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation).DisposeWith(_setupDisposable);
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

    public class ConnectionFactory
    {
        public virtual PendingConnectionViewModel CreatePendingConnection(NetworkViewModel network)
            => new PendingConnectionViewModel(network);

        public virtual ConnectionViewModel CreateConnection(NetworkViewModel network, NodeInputViewModel input, NodeOutputViewModel output)
            => new ConnectionViewModel(network, input, output);
    }
}
