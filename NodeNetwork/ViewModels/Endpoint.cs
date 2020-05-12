using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
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
	[DataContract]
	public enum PortPosition
	{
		[DataMember] Left, [DataMember] Right
	}

	/// <summary>
	/// Enum types that indicates the visibility behaviour of an endpoint
	/// </summary>
	[DataContract]
	public enum EndpointVisibility
	{
		/// <summary>
		/// Automatically decide whether or not to show this endpoint based on the collapse status of the node
		/// </summary>
		[DataMember] Auto,
		/// <summary>
		/// Always show this endpoint, even if the node is collapsed
		/// </summary>
		[DataMember] AlwaysVisible,
		/// <summary>
		/// Always hide this endpoint
		/// </summary>
		[DataMember] AlwaysHidden
	}

	/// <summary>
	/// Parent interface for the inputs/outputs of nodes between which connections can be made.
	/// </summary>
	[DataContract]
	public abstract class Endpoint : ReactiveObject, IHaveId
	{
		#region Serialisation Properties
		[DataMember]
		public string Id { get; set; }

		[DataMember] public string ParentId => Parent?.Id ?? string.Empty;
		#endregion

		#region Parent
		/// <summary>
		/// The node that owns this endpoint
		/// </summary>
		[DataMember]
		public NodeViewModel Parent
		{
			get => _parent;
			internal set => this.RaiseAndSetIfChanged(ref _parent, value);
		}
		[IgnoreDataMember] private NodeViewModel _parent;
		#endregion

		#region Name
		/// <summary>
		/// The name of this endpoint.
		/// In the default view, this string is displayed in the node next to the port.
		/// </summary>
		[DataMember]
		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}
		[IgnoreDataMember] private string _name = "";
		#endregion

		#region Icon
		/// <summary>
		/// The icon displayed near the endpoint label
		/// If this is null, no icon is displayed.
		/// </summary>
		[DataMember]
		public IBitmap Icon
		{
			get => _icon;
			set => this.RaiseAndSetIfChanged(ref _icon, value);
		}
		[IgnoreDataMember] private IBitmap _icon;
		#endregion

		#region Editor
		/// <summary>
		/// The editor viewmodel associated with this endpoint. 
		/// It can be used to configure the behaviour of this endpoint or provide a default value when there is no connection.
		/// The editor, if not null, will be displayed in the node, under the endpoint name next to the port.
		/// </summary>
		[DataMember]
		public NodeEndpointEditorViewModel Editor
		{
			get => _editor;
			set => this.RaiseAndSetIfChanged(ref _editor, value);
		}
		[IgnoreDataMember] private NodeEndpointEditorViewModel _editor;
		#endregion

		#region Port
		/// <summary>
		/// The viewmodel for the port of this endpoint. (the part the user can create connections from.)
		/// </summary>
		[DataMember]
		public PortViewModel Port
		{
			get => _port;
			set => this.RaiseAndSetIfChanged(ref _port, value);
		}
		[IgnoreDataMember] private PortViewModel _port;
		#endregion

		#region PortPosition
		/// <summary>
		/// Where should the port be positioned in the endpoint?
		/// </summary>
		[DataMember]
		public PortPosition PortPosition
		{
			get => _portPosition;
			set => this.RaiseAndSetIfChanged(ref _portPosition, value);
		}
		[IgnoreDataMember] private PortPosition _portPosition;
		#endregion

		#region Connections
		/// <summary>
		/// List of connections between this endpoint and other endpoints in the network.
		/// To add a new connection, do not add it here but instead add it to the Connections property in the network.
		/// </summary>
		[IgnoreDataMember] public IObservableList<ConnectionViewModel> Connections { get; }
		#endregion

		#region MaxConnections
		/// <summary>
		/// The maximum amount of connections this endpoint accepts.
		/// When Connections.Count == MaxConnections, the user cannot add more connections to this endpoint
		/// until a connection is removed.
		/// </summary>
		[DataMember]
		public int MaxConnections
		{
			get => _maxConnections;
			set => this.RaiseAndSetIfChanged(ref _maxConnections, value);
		}
		[IgnoreDataMember] private int _maxConnections;
		#endregion

		#region Visibility
		/// <summary>
		/// Visibility behaviour of this endpoint
		/// </summary>
		[DataMember]
		public EndpointVisibility Visibility
		{
			get => _visibility;
			set => this.RaiseAndSetIfChanged(ref _visibility, value);
		}
		[IgnoreDataMember] private EndpointVisibility _visibility;
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
