using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Aggregation;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{

	/// <summary>
	/// Represents a connection between a node input and a node output
	/// </summary>
	[DataContract]
	public class ConnectionViewModel : ReactiveObject, IHaveId
	{
		static ConnectionViewModel()
		{
			Splat.Locator.CurrentMutable.Register(() => new ConnectionView(), typeof(IViewFor<ConnectionViewModel>));
		}

		#region Logger
		[IgnoreDataMember] private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		/// <summary>
		/// The viewmodel of the node input that is on one end of the connection.
		/// </summary>
		[DataMember] public NodeInputViewModel Input { get; set; }

		/// <summary>
		/// The viewmodel of the node output that is on one end of the connection.
		/// </summary>
		[DataMember] public NodeOutputViewModel Output { get; set; }

		#region CanBeRemovedByUser
		/// <summary>
		/// If false, the user cannot delete this connection. True by default.
		/// </summary>
		[DataMember]
		public bool CanBeRemovedByUser
		{
			get => _canBeRemovedByUser;
			set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
		}
		[IgnoreDataMember] private bool _canBeRemovedByUser;
		#endregion

		#region Serialisation properties
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
		/// The network that contains this connection
		/// </summary>
		[DataMember]
		public NetworkViewModel Parent
		{
			get => _parent;
			set => this.RaiseAndSetIfChanged(ref _parent, value);
		}
		[IgnoreDataMember] private NetworkViewModel _parent;

		/// <summary>
		/// Rebuilds the specified parent.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="input">The input.</param>
		/// <param name="output">The output.</param>
		public void Rebuild(NetworkViewModel parent, NodeInputViewModel input, NodeOutputViewModel output)
		{
			Parent = parent;
			if (input != null)
			{
				Input = input;
			}
			if (output != null)
			{
				Output = output;
			}
		}
		#endregion

		#region IsHighlighted
		/// <summary>
		/// If true, the connection is highlighted.
		/// </summary>
		[DataMember]
		public bool IsHighlighted
		{
			get => _isHighlighted;
			set => this.RaiseAndSetIfChanged(ref _isHighlighted, value);
		}
		[IgnoreDataMember] private bool _isHighlighted;
		#endregion

		#region IsInErrorState
		/// <summary>
		/// If true, the connection is displayed as being in an erroneous state.
		/// </summary>
		[DataMember]
		public bool IsInErrorState
		{
			get => _isInErrorState;
			set => this.RaiseAndSetIfChanged(ref _isInErrorState, value);
		}
		[IgnoreDataMember] private bool _isInErrorState;
		#endregion

		#region IsMarkedForDelete
		/// <summary>
		/// If true, the connection is displayed as being marked for deletion.
		/// </summary>
		[DataMember] public bool IsMarkedForDelete => _isMarkedForDelete.Value;
		[IgnoreDataMember] private ObservableAsPropertyHelper<bool> _isMarkedForDelete;
		#endregion

		public ConnectionViewModel(NetworkViewModel parent, NodeInputViewModel input, NodeOutputViewModel output)
		{
			Parent = parent;
			Input = input;
			Output = output;
			
			this.WhenAnyValue(v => v.Parent.CutLine.IntersectingConnections)
				.Where(l => l != null)
				.Select(list => list.Connect().Filter(c => c == this).Count().Select(c => c > 0))
				.Switch()
				.ToProperty(this, vm => vm.IsMarkedForDelete, out _isMarkedForDelete);
		}
	}
}
