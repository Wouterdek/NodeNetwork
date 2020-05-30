using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows;
using DynamicData;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace NodeNetwork.ViewModels
{
	/// <summary>
	/// Viewmodel class for the nodes in the network
	/// </summary>
	[DataContract]
	public class NodeViewModel : ReactiveObject, INodeViewModel
	{
		static NodeViewModel()
		{
			Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<NodeViewModel>));
			Locator.CurrentMutable.RegisterPlatformBitmapLoader();
		}

		#region Logger
		[IgnoreDataMember] private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Serialisation Properties
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[DataMember]
		public string Id { get; set; }

		/// <summary>
		/// Gets the parent identifier.
		/// </summary>
		/// <value>
		/// The parent identifier.
		/// </value>
		[DataMember] public string ParentId => Parent?.Id ?? string.Empty;

		/// <summary>
		/// Gets a value indicating whether this instance is in design mode.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is in design mode; otherwise, <c>false</c>.
		/// </value>
		[IgnoreDataMember] protected bool InDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

		/// <summary>
		/// The rendered size of this node.
		/// </summary>
		[DataMember]
		public bool IsReadOnly
		{
			get => _readOnly;
			set
			{
				this.RaiseAndSetIfChanged(ref _readOnly, value);
			}
		}
		[IgnoreDataMember] private bool _readOnly;

		/// <summary>
		/// Rebuilds the specified parent.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public void Rebuild(NetworkViewModel parent)
		{
			Parent = parent;
		}
		#endregion

		#region Parent
		/// <summary>
		/// The network that contains this node
		/// </summary>
		[DataMember]
		public NetworkViewModel Parent
		{
			get => _parent;
			set => this.RaiseAndSetIfChanged(ref _parent, value);
		}
		[IgnoreDataMember] private NetworkViewModel _parent;
		#endregion

		#region Name
		/// <summary>
		/// The name of the node.
		/// In the default view, this string is displayed at the top of the node.
		/// </summary>
		[DataMember]
		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}
		[IgnoreDataMember] private string _name;
		#endregion

		#region HeaderIcon
		/// <summary>
		/// The icon displayed in the header of the node.
		/// If this is null, no icon is displayed.
		/// In the default view, this icon is displayed at the top of the node.
		/// </summary>
		[DataMember]
		public IBitmap HeaderIcon
		{
			get => _headerIcon;
			set => this.RaiseAndSetIfChanged(ref _headerIcon, value);
		}
		[IgnoreDataMember] private IBitmap _headerIcon;
		#endregion

		#region Inputs
		/// <summary>
		/// The list of inputs on this node.
		/// </summary>
		[DataMember] public ISourceList<NodeInputViewModel> Inputs { get; set; } = new SourceList<NodeInputViewModel>();
		#endregion

		#region Outputs
		/// <summary>
		/// The list of outputs on this node.
		/// </summary>
		[DataMember] public ISourceList<NodeOutputViewModel> Outputs { get; set; } = new SourceList<NodeOutputViewModel>();
		#endregion

		#region VisibleInputs
		/// <summary>
		/// The list of inputs that is currently visible on this node.
		/// Some inputs may be hidden if the node is collapsed.
		/// </summary>
		[IgnoreDataMember] public IObservableList<NodeInputViewModel> VisibleInputs { get; }
		#endregion

		#region VisibleOutputs
		/// <summary>
		/// The list of outputs that is currently visible on this node.
		/// Some outputs may be hidden if the node is collapsed.
		/// </summary>
		[IgnoreDataMember] public IObservableList<NodeOutputViewModel> VisibleOutputs { get; }
		#endregion

		#region VisibleEndpointGroups
		/// <summary>
		/// The list of endpoint groups that is currently visible on this node.
		/// Some groups may be hidden if the node is collapsed.
		/// </summary>
		[IgnoreDataMember] 
		public ReadOnlyObservableCollection<EndpointGroupViewModel> VisibleEndpointGroups { get; }
		#endregion

		#region EndpointGroupViewModelFactory
		/// <summary>
		/// The function that is used to create endpoint group view models.
		/// By default, this function creates a EndpointGroupViewModel.
		/// </summary>
		[IgnoreDataMember]
		public EndpointGroupViewModelFactory EndpointGroupViewModelFactory
		{
			get => _endpointGroupViewModelFactory;
			set => this.RaiseAndSetIfChanged(ref _endpointGroupViewModelFactory, value);
		}
		[IgnoreDataMember] private EndpointGroupViewModelFactory _endpointGroupViewModelFactory;
		#endregion

		#region IsSelected
		/// <summary>
		/// If true, this node is currently selected in the UI.
		/// </summary>
		[DataMember]
		public bool IsSelected
		{
			get => _isSelected;
			set => this.RaiseAndSetIfChanged(ref _isSelected, value);
		}
		[IgnoreDataMember] private bool _isSelected;
		#endregion

		#region IsCollapsed
		/// <summary>
		/// If true, this node is currently collapsed.
		/// If the node is collapsed, some parts of the node are hidden to provide a more compact view.
		/// </summary>
		[DataMember]
		public bool IsCollapsed
		{
			get => _isCollapsed;
			set => this.RaiseAndSetIfChanged(ref _isCollapsed, value);
		}
		[IgnoreDataMember] private bool _isCollapsed;
		#endregion

		#region CanBeRemovedByUser
		/// <summary>
		/// If true, the user can delete this node from the network in the UI.
		/// True by default.
		/// </summary>
		[DataMember]
		public bool CanBeRemovedByUser
		{
			get => _canBeRemovedByUser;
			set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
		}
		[IgnoreDataMember] private bool _canBeRemovedByUser;
		#endregion

		#region Position
		/// <summary>
		/// The position of this node in the network.
		/// </summary>
		[DataMember]
		public Point Position
		{
			get => _position;
			set => this.RaiseAndSetIfChanged(ref _position, value);
		}
		[IgnoreDataMember] private Point _position;
		#endregion

		#region Size
		/// <summary>
		/// The rendered size of this node.
		/// </summary>
		[DataMember]
		public Size Size
		{
			get => _size;
			set => this.RaiseAndSetIfChanged(ref _size, value);
		}
		[IgnoreDataMember] private Size _size;
		#endregion

		public NodeViewModel()
		{
			Id = Id ?? Guid.NewGuid().ToString();

			// Setup a default EndpointGroupViewModelFactory that will be used to create endpoint groups.
			EndpointGroupViewModelFactory = (group, allInputs, allOutputs, children, factory) => new EndpointGroupViewModel(group, allInputs, allOutputs, children, factory) { Id = Guid.NewGuid().ToString() , Parent = this};

			this.Name = "Untitled";
			this.CanBeRemovedByUser = true;

			// Setup parent relationship with inputs.
			Inputs.Connect().ActOnEveryObject(
				addedInput => addedInput.Parent = this,
				removedInput => removedInput.Parent = null
			);
			
			// Setup parent relationship with outputs.
			Outputs.Connect().ActOnEveryObject(
				addedOutput => addedOutput.Parent = this,
				removedOutput => removedOutput.Parent = null
			);
			
			// When an input is removed, delete any connection to/from that input
			Inputs.Preview().OnItemRemoved(removedInput =>
			{
				if (Parent != null)
				{
					Parent.Connections.RemoveMany(removedInput.Connections.Items); 

					bool pendingConnectionInvalid = Parent.PendingConnection?.Input == removedInput;
					if (pendingConnectionInvalid)
					{
						Parent.RemovePendingConnection();
					}
				}
			}).Subscribe();

			// Same for outputs.
			Outputs.Preview().OnItemRemoved(removedOutput =>
			{
				if (Parent != null)
				{
					Parent.Connections.RemoveMany(removedOutput.Connections.Items);

					bool pendingConnectionInvalid = Parent.PendingConnection?.Output == removedOutput;
					if (pendingConnectionInvalid)
					{
						Parent.RemovePendingConnection();
					}
				}
			}).Subscribe();
			
			// If collapsed, hide inputs without connections, otherwise show all.
			var onCollapseChange = this.WhenAnyValue(vm => vm.IsCollapsed).Publish();
			onCollapseChange.Connect();

			var visibilityFilteredInputs = Inputs.Connect()
				.AutoRefreshOnObservable(_ => onCollapseChange)
				.AutoRefresh(vm => vm.Visibility)
				.AutoRefresh(vm => vm.Group)
				.Filter(i =>
				{
					if (IsCollapsed)
					{
						return i.Visibility == EndpointVisibility.AlwaysVisible || (i.Visibility == EndpointVisibility.Auto && i.Connections.Items.Any());
					}
					return i.Visibility != EndpointVisibility.AlwaysHidden;
				});

			VisibleInputs = visibilityFilteredInputs.Filter(i => i.Group == null).AsObservableList();

			// Same for outputs.
			var visibilityFilteredOutputs = Outputs.Connect()
				.AutoRefreshOnObservable(_ => onCollapseChange)
				.AutoRefresh(vm => vm.Visibility)
				.AutoRefresh(vm => vm.Group)
				.Filter(o =>
				{
					if (IsCollapsed)
					{
						return o.Visibility == EndpointVisibility.AlwaysVisible || (o.Visibility == EndpointVisibility.Auto && o.Connections.Items.Any());
					}

					return o.Visibility != EndpointVisibility.AlwaysHidden;
				});
			VisibleOutputs = visibilityFilteredOutputs.Filter(o => o.Group == null).AsObservableList();

			// Grouping of all endpoints.
			var allEndpointGroups = visibilityFilteredInputs.Transform(i => i.Group).Merge(visibilityFilteredOutputs.Transform(o => o.Group));

			var allGroups = allEndpointGroups
				.TransformMany(group =>
				{
					var hierarchy = new List<EndpointGroup>();
					while (group != null)
					{
						hierarchy.Add(group);
						group = group.Parent;
					}
					return hierarchy;
				},
				EqualityComparer<EndpointGroup>.Default);

			// Used as temporary root for TransformToTree.
			var root = new EndpointGroup();

			// To react on change of the EndpointGroupViewModelFactory.
			var onEndpointGroupViewModelFactoryChange = this.WhenAnyValue(vm => vm.EndpointGroupViewModelFactory).Publish();
			onEndpointGroupViewModelFactoryChange.Connect();

			allGroups.AddKey(group => group)
				.TransformToTree(group => group.Parent ?? root)
				.AutoRefreshOnObservable(_ => onEndpointGroupViewModelFactoryChange)
				.Transform(n => EndpointGroupViewModelFactory(n.Key, visibilityFilteredInputs, visibilityFilteredOutputs, n.Children, EndpointGroupViewModelFactory))
				.Bind(out var groups).Subscribe();

			VisibleEndpointGroups = groups;
		}
	}
}
