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
        private NodeViewModel _parent;
        public NodeViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        #endregion

        #region Name
        private string _name = "";
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        #endregion
        
        #region ToolTip
        private object _toolTip;
        public object ToolTip
        {
            get => _toolTip;
            set => this.RaiseAndSetIfChanged(ref _toolTip, value);
        }
        #endregion
        
        #region Editor
        private NodeEndpointEditorViewModel _editor;
        public NodeEndpointEditorViewModel Editor
        {
            get => _editor;
            set => this.RaiseAndSetIfChanged(ref _editor, value);
        }
        #endregion

        #region IsEditorVisible
        private ObservableAsPropertyHelper<bool> _isEditorVisible;
        public bool IsEditorVisible => _isEditorVisible.Value;
        #endregion

        #region HideEditorIfConnected
        private bool _hideEditorIfConnected;
        public bool HideEditorIfConnected
        {
            get => _hideEditorIfConnected;
            set => this.RaiseAndSetIfChanged(ref _hideEditorIfConnected, value);
        }
        #endregion
        
        #region Port
        public PortViewModel Port { get; } = new PortViewModel();
        #endregion
        
        #region ValueFactory
        private Func<NodeInputViewModel, object> _valueFactory;
        public Func<NodeInputViewModel, object> ValueFactory
        {
            get => _valueFactory;
            set => this.RaiseAndSetIfChanged(ref _valueFactory, value);
        }
        #endregion
        
        #region ConnectionValidator
        private Func<PendingConnectionViewModel, ConnectionValidationResult> _connectionValidator;
        public Func<PendingConnectionViewModel, ConnectionValidationResult> ConnectionValidator
        {
            get => _connectionValidator;
            set => this.RaiseAndSetIfChanged(ref _connectionValidator, value);
        }
        #endregion
        
        #region Connection
        private ObservableAsPropertyHelper<ConnectionViewModel> _connection;
        public ConnectionViewModel Connection => _connection.Value;
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
