using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using NodeNetwork.Utilities;
using ReactiveUI;
using Splat;

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

        #region Group

        /// <summary>
        /// The group the end point belongs to. Can be null.
        /// </summary>
        public EndpointGroup Group
        {
            get => _group;
            set => this.RaiseAndSetIfChanged(ref _group, value);
        }
        private EndpointGroup _group;
        #endregion

        #region Icon
        /// <summary>
        /// The icon displayed near the endpoint label
        /// If this is null, no icon is displayed.
        /// </summary>
        public IBitmap Icon
        {
            get => _icon;
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }
        private IBitmap _icon;
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
        public IObservableList<ConnectionViewModel> Connections { get; }
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

        #region SortIndex
        /// <summary>
        /// Inputs and outputs are sorted by increasing values of SortIndex before being displayed.
        /// </summary>
        public int SortIndex
        {
            get => _sortIndex;
            set => this.RaiseAndSetIfChanged(ref _sortIndex, value);
        }
        private int _sortIndex;
        #endregion

        protected Endpoint()
        {
            Port = new PortViewModel();
            Visibility = EndpointVisibility.Auto;

            // Setup parent relationship with Port.
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

            // Setup Parent relationship with Editor.
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

            // Mirror the port if the endpoint is on the left instead of the right.
            this.WhenAnyValue(vm => vm.Port, vm => vm.PortPosition).Subscribe(_ =>
            {
                if (Port == null)
                {
                    return;
                }

                Port.IsMirrored = PortPosition == PortPosition.Left;
            });

            // Setup a binding between the Connections list in the network and in this endpoint,
            // selecting only the connections where this endpoint is the input or output.

            // We need the latest network connections list, but we want a null value when this endpoint is
            // removed from the node, or the node is removed from the network.
            var networkConnections = this.WhenAnyValue(
                vm => vm.Parent, 
                vm => vm.Parent.Parent,
                vm => vm.Parent.Parent.Connections, 
                (x, y, z) => Parent?.Parent?.Connections ?? new SourceList<ConnectionViewModel>())
                .Switch();

	        Connections = networkConnections
				.AutoRefresh(c => c.Input)
				.AutoRefresh(c => c.Output)
				.Filter(c => c.Input == this || c.Output == this)
				.AsObservableList();

            // Setup bindings between port mouse events and connection creation.
            this.WhenAnyObservable(vm => vm.Port.ConnectionDragStarted).Subscribe(_ => CreatePendingConnection());
            this.WhenAnyObservable(vm => vm.Port.ConnectionPreviewActive).Subscribe(SetConnectionPreview);
            this.WhenAnyObservable(vm => vm.Port.ConnectionDragFinished).Subscribe(_ => FinishPendingConnection());
        }

        protected abstract void CreatePendingConnection();
        protected abstract void SetConnectionPreview(bool previewActive);
        protected abstract void FinishPendingConnection();
    }
}
