using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for outputs on a node.
    /// Outputs are endpoints that can only be connected to inputs.
    /// </summary>
    public class NodeOutputViewModel : ReactiveObject, IEndpoint
    {
        static NodeOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NodeOutputViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The node that owns this output
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
        /// The name of this output.
        /// In the default view, this string is displayed in the node next to the output port.
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
        /// The editor viewmodel associated with this output. It can be used to configure the behaviour of this output.
        /// The editor, if not null, will be displayed in the node, under the output name next to the output port.
        /// </summary>
        public NodeEndpointEditorViewModel Editor
        {
            get => _editor;
            set => this.RaiseAndSetIfChanged(ref _editor, value);
        }
        private NodeEndpointEditorViewModel _editor;
        #endregion

        #region Port
        /// <summary>
        /// The viewmodel for the port of this output. (the part the user can create connections from.)
        /// </summary>
        public PortViewModel Port { get; } = new PortViewModel();
        #endregion
        
        #region Connections
        /// <summary>
        /// List of connections between this output and other inputs in the network.
        /// To add a new connection, do not add it here but instead add it to the Connections property in the network.
        /// </summary>
        public IReactiveList<ConnectionViewModel> Connections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion

        public NodeOutputViewModel()
        {
            Port.Parent = this;

            this.WhenAnyObservable(vm => vm.Parent.Parent.Connections.Changing)
                .Where(_ => Parent?.Parent != null) //Chained WhenAnyObservable calls dont unsubscribe when an element in the chain becomes null (ReactiveUI #769)
                .Select(change => GetConnectionChanges(Connections, change))
                .Where(t => t.hasChanged)
                .Select(t => t.connections.ToList())
                .BindListContents(this, vm => vm.Connections);

            this.Port.ConnectionDragStarted.Subscribe(_ => StartConnectionDrag());
            this.Port.ConnectionPreviewActive.Subscribe(previewActive =>
            {
                PendingConnectionViewModel pendingCon = Parent.Parent.PendingConnection;
                if (pendingCon.Output != null && (pendingCon.Output != this || pendingCon.OutputIsLocked))
                {
                    return;
                }

                if (previewActive)
                {
                    pendingCon.Output = this;
                    pendingCon.Validation = pendingCon.Input.ConnectionValidator(pendingCon);
                }
                else
                {
                    pendingCon.Output = null;
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

            network.PendingConnection = new PendingConnectionViewModel(network){ Output = this, OutputIsLocked = true, LooseEndPoint = Port.CenterPoint };
        }

        private void EndConnectionDrag()
        {
            NetworkViewModel network = Parent?.Parent;
            if (network == null)
            {
                return;
            }

            if (network.PendingConnection.Output == this && !network.PendingConnection.OutputIsLocked)
            {
                //Only allow drag from output to input, not input to input
                if (network.PendingConnection.Input.Parent != network.PendingConnection.Output.Parent)
                {
                    //Dont allow connections between an input and an output on the same node
                    if (network.PendingConnection.Validation.IsValid)
                    {
                        //Connection is valid
                        if (!network.Connections.Any(con => con.Output == this && con.Input == network.PendingConnection.Input))
                        {
                            //Connection does not exist already
                            network.Connections.Add(network.ConnectionFactory(network.PendingConnection.Input, this));
                        }
                    }
                }
            }

            network.RemovePendingConnection();
        }

        private (bool hasChanged, IEnumerable<ConnectionViewModel> connections) GetConnectionChanges(IList<ConnectionViewModel> curConnections, NotifyCollectionChangedEventArgs change)
        {
            if (change.Action == NotifyCollectionChangedAction.Reset)
            {
                return (true, Enumerable.Empty<ConnectionViewModel>());
            }
            else if (change.Action == NotifyCollectionChangedAction.Add)
            {
                IList<ConnectionViewModel> newConnections = change.NewItems.OfType<ConnectionViewModel>().Where(c => c.Output == this).ToList();
                if (newConnections.Count > 0)
                {
                    return (true, curConnections.Concat(newConnections));
                }
            }
            else if (change.Action == NotifyCollectionChangedAction.Remove)
            {
                IList<ConnectionViewModel> filteredConnections = curConnections.Where(c => !change.OldItems.OfType<ConnectionViewModel>().Contains(c)).ToList();
                if (filteredConnections.Count != curConnections.Count)
                {
                    return (true, filteredConnections);
                }
            }
            else if (change.Action == NotifyCollectionChangedAction.Replace)
            {
                IList<ConnectionViewModel> filteredConnections = curConnections.Where(c => !change.OldItems.OfType<ConnectionViewModel>().Contains(c)).ToList();
                IList<ConnectionViewModel> newConnections = change.NewItems.OfType<ConnectionViewModel>().Where(c => c.Output == this).ToList();
                if (filteredConnections.Count != curConnections.Count || newConnections.Count > 0)
                {
                    return (true, filteredConnections.Concat(newConnections));
                }
            }
            return (false, null);
        }
    }
}
