using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Utilities;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Enum type that indicates the position of the port in the endpoint
    /// </summary>
    public enum PortPosition
    {
        Left, Right
    }

    /// <summary>
    /// Enum types that indicates the visibility behaviour of an endpoint
    /// </summary>
    public enum EndpointVisibility
    {
        /// <summary>
        /// Automatically decide whether or not to show this endpoint based on the collapse status of the node
        /// </summary>
        Auto,
        /// <summary>
        /// Always show this endpoint, even if the node is collapsed
        /// </summary>
        AlwaysVisible,
        /// <summary>
        /// Always hide this endpoint
        /// </summary>
        AlwaysHidden
    }

    /// <summary>
    /// Parent interface for the inputs/outputs of nodes between which connections can be made.
    /// </summary>
    public abstract class Endpoint : ReactiveObject
    {
        #region Parent
        /// <summary>
        /// The node that owns this endpoint
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
        /// The name of this endpoint.
        /// In the default view, this string is displayed in the node next to the port.
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
        /// The editor viewmodel associated with this endpoint. 
        /// It can be used to configure the behaviour of this endpoint or provide a default value when there is no connection.
        /// The editor, if not null, will be displayed in the node, under the endpoint name next to the port.
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
        /// The viewmodel for the port of this endpoint. (the part the user can create connections from.)
        /// </summary>
        public PortViewModel Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }
        private PortViewModel _port;
        #endregion
        
        #region PortPosition
        /// <summary>
        /// Where should the port be positioned in the endpoint?
        /// </summary>
        public PortPosition PortPosition
        {
            get => _portPosition;
            set => this.RaiseAndSetIfChanged(ref _portPosition, value);
        }
        private PortPosition _portPosition;
        #endregion

        #region Connections
        /// <summary>
        /// List of connections between this endpoint and other endpoints in the network.
        /// To add a new connection, do not add it here but instead add it to the Connections property in the network.
        /// </summary>
        public IReadOnlyReactiveList<ConnectionViewModel> Connections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion

        #region MaxConnections
        /// <summary>
        /// The maximum amount of connections this endpoint accepts.
        /// When Connections.Count == MaxConnections, the user cannot add more connections to this endpoint
        /// until a connection is removed.
        /// </summary>
        public int MaxConnections
        {
            get => _maxConnections;
            set => this.RaiseAndSetIfChanged(ref _maxConnections, value);
        }
        private int _maxConnections;
        #endregion

        #region Visibility
        /// <summary>
        /// Visibility behaviour of this endpoint
        /// </summary>
        public EndpointVisibility Visibility
        {
            get => _visibility;
            set => this.RaiseAndSetIfChanged(ref _visibility, value);
        }
        private EndpointVisibility _visibility;
        #endregion
        
        protected Endpoint()
        {
            Port = new PortViewModel();
            this.WhenAnyValue(vm => vm.Port).PairWithPreviousValue().Subscribe(p =>
            {
                if (p.OldValue != null)
                {
                    p.OldValue.Parent = null;
                }

                if (p.NewValue != null)
                {
                    p.NewValue.Parent = this;
                }
            });

            this.WhenAnyValue(vm => vm.Editor).PairWithPreviousValue().Subscribe(e =>
            {
                if (e.OldValue != null)
                {
                    e.OldValue.Parent = null;
                }

                if (e.NewValue != null)
                {
                    e.NewValue.Parent = this;
                }
            });

            this.WhenAnyValue(vm => vm.Port, vm => vm.PortPosition).Subscribe(t =>
                {
                    if (t.Item1 == null)
                    {
                        return;
                    }
                    t.Item1.IsMirrored = t.Item2 == PortPosition.Left;
                });
            
            this.WhenAnyValue(vm => vm.Parent.Parent.Connections)
                .Select(l => l.Where(c => c.Input == this || c.Output == this).ToList())
                .Merge(this.WhenAnyObservable(vm => vm.Parent.Parent.Connections.Changing)
                    .Where(_ => Parent?.Parent != null) //Chained WhenAnyObservable calls dont unsubscribe when an element in the chain becomes null (ReactiveUI #769)
                    .Select(change => GetConnectionChanges(Connections, change))
                    .Where(t => t.hasChanged)
                    .Select(t => t.connections.ToList()))
                .BindListContents(this, vm => vm.Connections);

            this.WhenAnyObservable(vm => vm.Port.ConnectionDragStarted).Subscribe(_ => CreatePendingConnection());
            this.WhenAnyObservable(vm => vm.Port.ConnectionPreviewActive).Subscribe(SetConnectionPreview);
            this.WhenAnyObservable(vm => vm.Port.ConnectionDragFinished).Subscribe(_ => FinishPendingConnection());

            Visibility = EndpointVisibility.Auto;
        }

        protected abstract void CreatePendingConnection();
        protected abstract void SetConnectionPreview(bool previewActive);
        protected abstract void FinishPendingConnection();

        private (bool hasChanged, IEnumerable<ConnectionViewModel> connections) GetConnectionChanges(IReadOnlyList<ConnectionViewModel> curConnections, NotifyCollectionChangedEventArgs change)
        {
            if (change.Action == NotifyCollectionChangedAction.Reset)
            {
                return (true, Enumerable.Empty<ConnectionViewModel>());
            }
            else if (change.Action == NotifyCollectionChangedAction.Add)
            {
                IList<ConnectionViewModel> newConnections = change.NewItems.OfType<ConnectionViewModel>().Where(c => c.Input == this || c.Output == this).ToList();
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
                IList<ConnectionViewModel> newConnections = change.NewItems.OfType<ConnectionViewModel>().Where(c => c.Input == this || c.Output == this).ToList();
                if (filteredConnections.Count != curConnections.Count || newConnections.Count > 0)
                {
                    return (true, filteredConnections.Concat(newConnections));
                }
            }
            return (false, null);
        }
    }
}
