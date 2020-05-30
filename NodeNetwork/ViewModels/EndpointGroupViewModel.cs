using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
	[DataContract]
	public class EndpointGroupViewModel : ReactiveObject
	{
		static EndpointGroupViewModel()
		{
			Splat.Locator.CurrentMutable.Register(() => new EndpointGroupView(), typeof(IViewFor<EndpointGroupViewModel>));
		}

		#region Serialisation Properties
		[DataMember]
		public string Id { get; set; }

		[DataMember] public string ParentId => Parent?.Id ?? string.Empty;

		/// <summary>
		/// The network that contains this connection
		/// </summary>
		[DataMember]
		public NodeViewModel Parent
		{
			get => _parent;
			set => this.RaiseAndSetIfChanged(ref _parent, value);
		}
		[IgnoreDataMember] private NodeViewModel _parent;
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

		#region Group
		/// <summary>
		/// The endpoint group wrapping the name and the parent group of this group.
		/// </summary>
		[DataMember]
		public EndpointGroup Group
		{
			get => _group;
			set => this.RaiseAndSetIfChanged(ref _group, value);
		}
		[IgnoreDataMember] private EndpointGroup _group;
		#endregion

		#region Children
		/// <summary>
		/// The list of nested endpoint groups.
		/// </summary>
		[IgnoreDataMember] public ReadOnlyObservableCollection<EndpointGroupViewModel> Children => _children;
		[IgnoreDataMember] private readonly ReadOnlyObservableCollection<EndpointGroupViewModel> _children;
		#endregion

		public EndpointGroupViewModel(EndpointGroup group, IObservable<IChangeSet<NodeInputViewModel>> allInputs, IObservable<IChangeSet<NodeOutputViewModel>> allOutputs, IObservableCache<Node<EndpointGroup, EndpointGroup>, EndpointGroup> children, EndpointGroupViewModelFactory endpointGroupViewModelFactory)
		{
			Group = group;
			VisibleInputs = allInputs.Filter(e => e.Group == group).AsObservableList();
			VisibleOutputs = allOutputs.Filter(e => e.Group == group).AsObservableList();
			children
				.Connect()
				.Transform(n => endpointGroupViewModelFactory(n.Key, allInputs, allOutputs, n.Children, endpointGroupViewModelFactory))
				.Bind(out _children)
				.Subscribe();
		}
	}
}