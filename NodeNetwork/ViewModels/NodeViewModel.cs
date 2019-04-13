using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for the nodes in the network
    /// </summary>
    public class NodeViewModel : ReactiveObject
    {
        static NodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<NodeViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Parent
        /// <summary>
        /// The network that contains this node
        /// </summary>
        public NetworkViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        private NetworkViewModel _parent;
        #endregion

        #region Name
        /// <summary>
        /// The name of the node.
        /// In the default view, this string is displayed at the top of the node.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _name;
        #endregion

        #region Inputs
        /// <summary>
        /// The list of inputs on this node.
        /// </summary>
        public ISourceList<NodeInputViewModel> Inputs { get; } = new SourceList<NodeInputViewModel>();
		#endregion

		#region Outputs
		/// <summary>
		/// The list of outputs on this node.
		/// </summary>
		public ISourceList<NodeOutputViewModel> Outputs { get; } = new SourceList<NodeOutputViewModel>();
        #endregion

        #region VisibleInputs
        /// <summary>
        /// The list of inputs that is currently visible on this node.
        /// Some inputs may be hidden if the node is collapsed.
        /// </summary>
        public IObservableList<NodeInputViewModel> VisibleInputs { get; }
        #endregion

        #region VisibleOutputs
        /// <summary>
        /// The list of outputs that is currently visible on this node.
        /// Some outputs may be hidden if the node is collapsed.
        /// </summary>
        public IObservableList<NodeOutputViewModel> VisibleOutputs { get; }
        #endregion
        
        #region IsSelected
        /// <summary>
        /// If true, this node is currently selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        private bool _isSelected;
        #endregion

        #region IsCollapsed
        /// <summary>
        /// If true, this node is currently collapsed.
        /// If the node is collapsed, some parts of the node are hidden to provide a more compact view.
        /// </summary>
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => this.RaiseAndSetIfChanged(ref _isCollapsed, value);
        }
        private bool _isCollapsed;
        #endregion

        #region CanBeRemovedByUser
        /// <summary>
        /// If true, the user can delete this node from the network in the UI.
        /// True by default.
        /// </summary>
        public bool CanBeRemovedByUser
        {
            get => _canBeRemovedByUser;
            set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
        }
        private bool _canBeRemovedByUser;
        #endregion

        #region Position
        /// <summary>
        /// The position of this node in the network.
        /// </summary>
        public Point Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }
        private Point _position;
		#endregion

		#region Size
		/// <summary>
		/// The rendered size of this node.
		/// </summary>
		public Size Size
		{
			get => _size;
			internal set => this.RaiseAndSetIfChanged(ref _size, value);
		}
		private Size _size;
		#endregion

		public NodeViewModel()
        {
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

	        VisibleInputs = Inputs.Connect()
		        .AutoRefreshOnObservable(_ => onCollapseChange)
		        .AutoRefresh(vm => vm.Visibility)
		        .Filter(i =>
		        {
			        if (IsCollapsed)
			        {
				        return i.Visibility == EndpointVisibility.AlwaysVisible ||
				               (i.Visibility == EndpointVisibility.Auto && i.Connections.Items.Any());
			        }

			        return i.Visibility != EndpointVisibility.AlwaysHidden;
		        })
		        .AsObservableList();

			// Same for outputs.
			VisibleOutputs = Outputs.Connect()
				.AutoRefreshOnObservable(_ => onCollapseChange)
				.AutoRefresh(vm => vm.Visibility)
				.Filter(o =>
				{
					if (IsCollapsed)
					{
						return o.Visibility == EndpointVisibility.AlwaysVisible ||
						       (o.Visibility == EndpointVisibility.Auto && o.Connections.Items.Any());
					}

					return o.Visibility != EndpointVisibility.AlwaysHidden;
				})
				.AsObservableList();
        }
    }
}
