using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public IReactiveList<NodeInputViewModel> Inputs => _inputs;
        private readonly ReactiveList<NodeInputViewModel> _inputs = new ReactiveList<NodeInputViewModel> { ChangeTrackingEnabled = true };
        #endregion

        #region Outputs
        /// <summary>
        /// The list of outputs on this node.
        /// </summary>
        public IReactiveList<NodeOutputViewModel> Outputs => _outputs;
        private readonly ReactiveList<NodeOutputViewModel> _outputs = new ReactiveList<NodeOutputViewModel> { ChangeTrackingEnabled = true };
        #endregion

        #region VisibleInputs
        /// <summary>
        /// The list of inputs that is currently visible on this node.
        /// Some inputs may be hidden if the node is collapsed.
        /// </summary>
        public IReadOnlyReactiveList<NodeInputViewModel> VisibleInputs { get; } = new ReactiveList<NodeInputViewModel>();
        #endregion

        #region VisibleOutputs
        /// <summary>
        /// The list of outputs that is currently visible on this node.
        /// Some outputs may be hidden if the node is collapsed.
        /// </summary>
        public IReadOnlyReactiveList<NodeOutputViewModel> VisibleOutputs { get; } = new ReactiveList<NodeOutputViewModel>();
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
            Inputs.ActOnEveryObject(
                (NodeInputViewModel addedInput) => addedInput.Parent = this,
                removedInput => removedInput.Parent = null
            );

            // Setup parent relationship with outputs.
            Outputs.ActOnEveryObject(
                (NodeOutputViewModel addedOutput) => addedOutput.Parent = this,
                removedOutput => removedOutput.Parent = null
            );

            // When an input is removed, delete any connection to/from that input
            Inputs.ItemsRemoved.Where(_ => Parent != null).Subscribe(removedInput =>
            {
                Parent.Connections.RemoveAll(removedInput.Connections.ToArray());

                bool pendingConnectionInvalid = Parent.PendingConnection?.Input == removedInput;
                if (pendingConnectionInvalid)
                {
                    Parent.RemovePendingConnection();
                }
            });

            // Same for outputs.
            Outputs.ItemsRemoved.Where(_ => Parent != null).Subscribe(removedOutput =>
            {
                Parent.Connections.RemoveAll(removedOutput.Connections.ToArray());

                bool pendingConnectionInvalid = Parent.PendingConnection?.Output == removedOutput;
                if (pendingConnectionInvalid)
                {
                    Parent.RemovePendingConnection();
                }
            });

            // When the list of inputs is reset, remove any connections whose input node was removed.
            Inputs.ShouldReset.Where(_ => Parent != null).Subscribe(_ => CleanupInputOrphanedConnections());

            // When the list of outputs is reset, remove any connections whose output node was removed.
            Outputs.ShouldReset.Where(_ => Parent != null).Subscribe(_ => CleanupOutputOrphanedConnections());

            // If collapsed, hide inputs without connections, otherwise show all.
            Observable.CombineLatest(this.WhenAnyValue(vm => vm.IsCollapsed), this.WhenAnyObservable(vm => vm.Inputs.Changed), (a, b) => Unit.Default)
                .Select(_ =>
                {
                    if (IsCollapsed)
                    {
                        return Inputs.Where(i =>
                            i.Visibility == EndpointVisibility.AlwaysVisible ||
                            (i.Visibility == EndpointVisibility.Auto && !i.Connections.IsEmpty)
                        );
                    }
                    return Inputs.Where(i => i.Visibility != EndpointVisibility.AlwaysHidden);
                })
                .Select(e => e.ToList())
                .BindListContents(this, vm => vm.VisibleInputs);

            // Same for outputs.
            Observable.CombineLatest(this.WhenAnyValue(vm => vm.IsCollapsed), this.WhenAnyObservable(vm => vm.Outputs.Changed), (a, b) => Unit.Default)
                .Select(_ =>
                {
                    if (IsCollapsed)
                    {
                        return Outputs.Where(o =>
                            o.Visibility == EndpointVisibility.AlwaysVisible ||
                            (o.Visibility == EndpointVisibility.Auto && !o.Connections.IsEmpty)
                        );
                    }
                    return Outputs.Where(o => o.Visibility != EndpointVisibility.AlwaysHidden);
                })
                .Select(e => e.ToList())
                .BindListContents(this, vm => vm.VisibleOutputs);
        }
        
        private void CleanupInputOrphanedConnections()
        {
            // Create a hashset with all inputs for O(1) search
            HashSet<NodeInputViewModel> inputsSet = new HashSet<NodeInputViewModel>(Inputs);

            for (var i = Parent.Connections.Count - 1; i >= 0; i--)
            {
                var conn = Parent.Connections[i];

                // Remove any connection with inputs that were removed from this node.
                // Because the parent of the input can already be null (because it was removed from the node)
                // we must also remove connections with an input that have a null parent.
                if (conn.Input.Parent == null || (conn.Input.Parent == this && !inputsSet.Contains(conn.Input)))
                {
                    Parent.Connections.RemoveAt(i);
                }
            }

            var pendingConnInput = Parent.PendingConnection?.Input;
            if (pendingConnInput != null)
            {
                if (pendingConnInput.Parent == null || (pendingConnInput.Parent == this && !inputsSet.Contains(pendingConnInput)))
                {
                    Parent.RemovePendingConnection();
                }
            }
        }

        private void CleanupOutputOrphanedConnections()
        {
            // Create a hashset with all outputs for O(1) search
            HashSet<NodeOutputViewModel> outputsSet = new HashSet<NodeOutputViewModel>(Outputs);

            for (var i = Parent.Connections.Count - 1; i >= 0; i--)
            {
                var conn = Parent.Connections[i];

                // Remove any connection with outputs that were removed from this node.
                // Because the parent of the output can already be null (because it was removed from the node)
                // we must also remove connections with an output that have a null parent.
                if (conn.Output.Parent == null || (conn.Output.Parent == this && !outputsSet.Contains(conn.Output)))
                {
                    Parent.Connections.RemoveAt(i);
                }
            }

            var pendingConnOutput = Parent.PendingConnection?.Output;
            if (pendingConnOutput != null)
            {
                if (pendingConnOutput.Parent == null || (pendingConnOutput.Parent == this && !outputsSet.Contains(pendingConnOutput)))
                {
                    Parent.RemovePendingConnection();
                }
            }
        }
    }
}
