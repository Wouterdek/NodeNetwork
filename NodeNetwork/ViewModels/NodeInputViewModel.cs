using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;
using System.Reactive.Linq;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for inputs on a node.
    /// Inputs are endpoints that can only be connected to outputs.
    /// </summary>
    public class NodeInputViewModel : ReactiveObject, IEndpoint	
    {
        static NodeInputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NodeInputViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The node that owns this input
        /// </summary>
        public NodeViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        private NodeViewModel _parent;
        #endregion

        #region Name
        /// <summary>
        /// The name of this input.
        /// In the default view, this string is displayed in the node next to the input port.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _name = "";
        #endregion

        #region Editor
        /// <summary>
        /// The editor viewmodel associated with this input. 
        /// It can be used to configure the behaviour of this input or provide a default input when there is no connection.
        /// The editor, if not null, will be displayed in the node, under the input name next to the input port.
        /// </summary>
        public NodeEndpointEditorViewModel Editor
        {
            get => _editor;
            set => this.RaiseAndSetIfChanged(ref _editor, value);
        }
        private NodeEndpointEditorViewModel _editor;
        #endregion

        #region IsEditorVisible
        /// <summary>
        /// If true, the editor is visible. Otherwise, the editor is hidden.
        /// See HideEditorIfConnected.
        /// </summary>
        public bool IsEditorVisible => _isEditorVisible.Value;
        private ObservableAsPropertyHelper<bool> _isEditorVisible;
        #endregion

        #region HideEditorIfConnected
        /// <summary>
        /// If true, the editor of this input will be hidden if Connection is not null.
        /// This makes sense if the editor is used to provide a value when no connection is present.
        /// </summary>
        public bool HideEditorIfConnected
        {
            get => _hideEditorIfConnected;
            set => this.RaiseAndSetIfChanged(ref _hideEditorIfConnected, value);
        }
        private bool _hideEditorIfConnected;
        #endregion

        #region Port
        /// <summary>
        /// The viewmodel for the port of this input. (the part the user can create connections from.)
        /// </summary>
        public PortViewModel Port { get; } = new PortViewModel();
        #endregion

        #region ConnectionValidator
        /// <summary>
        /// This function is called when a new connection with this input is pending.
        /// It decides whether or not the pending connection is valid.
        /// If the validation result says the pending connection is invalid, 
        /// then the user will not be able to add the connection to the network.
        /// </summary>
        public Func<PendingConnectionViewModel, ConnectionValidationResult> ConnectionValidator
        {
            get => _connectionValidator;
            set => this.RaiseAndSetIfChanged(ref _connectionValidator, value);
        }
        private Func<PendingConnectionViewModel, ConnectionValidationResult> _connectionValidator;
        #endregion

        #region Connection
        /// <summary>
        /// The connection with this input in the network. Null if there is no such connection.
        /// </summary>
        public ConnectionViewModel Connection => _connection.Value;
        private ObservableAsPropertyHelper<ConnectionViewModel> _connection;
        #endregion

        public NodeInputViewModel()
        {
            Port.Parent = this;
            this.WhenAnyObservable(vm => vm.Parent.Parent.Connections.Changed)
                .Where(_ => Parent?.Parent != null) //Chained WhenAnyObservable calls dont unsubscribe when an element in the chain becomes null (ReactiveUI #769)
                .Select(GetConnectionChange)
                .Where(t => t.hasChanged)
                .Select(t => t.newCon)
                .ToProperty(this, vm => vm.Connection, out _connection);

            this.HideEditorIfConnected = true;
            this.WhenAny(vm => vm.HideEditorIfConnected, vm => vm.Connection,
                (x, y) => !(HideEditorIfConnected && Connection != null))
                .ToProperty(this, vm => vm.IsEditorVisible, out _isEditorVisible);

            this.ConnectionValidator = con => new ConnectionValidationResult(true, null);

            this.Port.ConnectionDragStarted.Subscribe(_ => StartConnectionDrag());
            this.Port.ConnectionPreviewActive.Subscribe(previewActive =>
            {
                PendingConnectionViewModel pendingCon = Parent.Parent.PendingConnection;
                if (pendingCon.Input != null && (pendingCon.Input != this || pendingCon.InputIsLocked))
                {
                    return;
                }

                if (previewActive)
                {
                    pendingCon.Input = this;
                    pendingCon.Validation = ConnectionValidator(pendingCon);
                }
                else
                {
                    pendingCon.Input = null;
                    pendingCon.Validation = null;
                }
            });
            this.Port.ConnectionDragFinished.Subscribe(_ => EndConnectionDrag());
        }

        private void StartConnectionDrag()
        {
            NetworkViewModel network = Parent?.Parent;
            if (network == null)
            {
                return;
            }

            PendingConnectionViewModel pendingConnection;
            if (Connection != null)
            {
                pendingConnection = new PendingConnectionViewModel(network)
                {
                    Output = Connection.Output, OutputIsLocked = true, LooseEndPoint = Port.CenterPoint
                };
                network.Connections.Remove(Connection);
            }
            else
            {
                pendingConnection = new PendingConnectionViewModel(network){ Input = this, InputIsLocked = true, LooseEndPoint = Port.CenterPoint };
            }
            pendingConnection.LooseEndPoint = Port.CenterPoint;
            network.PendingConnection = pendingConnection;
        }

        private void EndConnectionDrag()
        {
            NetworkViewModel network = Parent?.Parent;
            if (network == null)
            {
                return;
            }

            if (network.PendingConnection.Input == this && !network.PendingConnection.InputIsLocked)
            {
                //Only allow drag from output to input, not input to input
                if (network.PendingConnection.Input.Parent != network.PendingConnection.Output.Parent)
                {
                    //Dont allow connections between an input and an output on the same node
                    if (network.PendingConnection.Validation.IsValid)
                    {
                        //Connection is valid

                        ConnectionViewModel con = network.Connections.FirstOrDefault(c => c.Input == this);
                        if (con != null)
                        {
                            //Remove any connection to this input
                            network.Connections.Remove(con);
                        }

                        //Add new connection
                        network.Connections.Add(network.ConnectionFactory(this, network.PendingConnection.Output));
                    }
                }
            }
            network.RemovePendingConnection();
        }

        private (bool hasChanged, ConnectionViewModel newCon) GetConnectionChange(NotifyCollectionChangedEventArgs change)
        {
            if (change.Action == NotifyCollectionChangedAction.Reset)
            {
                return (true, null);
            }
            else if (change.Action == NotifyCollectionChangedAction.Add)
            {
                ConnectionViewModel newCon = change.NewItems.OfType<ConnectionViewModel>()
                    .FirstOrDefault(c => c.Input == this);
                if (newCon != null)
                {
                    return (true, newCon);
                }
            }
            else if (change.Action == NotifyCollectionChangedAction.Remove)
            {
                ConnectionViewModel con = change.OldItems.OfType<ConnectionViewModel>()
                    .FirstOrDefault(c => c.Input == this);
                if (con != null)
                {
                    return (true, null);
                }
            }
            else if (change.Action == NotifyCollectionChangedAction.Replace)
            {
                ConnectionViewModel newCon = change.NewItems.OfType<ConnectionViewModel>()
                    .FirstOrDefault(c => c.Input == this);
                if (newCon != null)
                {
                    return (true, newCon);
                }

                ConnectionViewModel con = change.OldItems.OfType<ConnectionViewModel>()
                    .FirstOrDefault(c => c.Input == this);
                if (con != null)
                {
                    return (true, null);
                }
            }
            return (false, null);
        }
    }
}
