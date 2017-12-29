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
        public IReactiveList<NodeInputViewModel> VisibleInputs { get; } = new ReactiveList<NodeInputViewModel>();
        #endregion

        #region VisibleOutputs
        /// <summary>
        /// The list of outputs that is currently visible on this node.
        /// Some outputs may be hidden if the node is collapsed.
        /// </summary>
        public IReactiveList<NodeOutputViewModel> VisibleOutputs { get; } = new ReactiveList<NodeOutputViewModel>();
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

        public NodeViewModel()
        {
            this.Name = "Untitled";

            Inputs.BeforeItemsAdded.Subscribe(input => input.Parent = this);
            Inputs.BeforeItemsRemoved.Subscribe(input => input.Parent = null);

            Outputs.BeforeItemsAdded.Subscribe(output => output.Parent = this);
            Outputs.BeforeItemsRemoved.Subscribe(output => output.Parent = null);

            //If collapsed, hide inputs/outputs without connections, otherwise show all
            Observable.CombineLatest(this.WhenAnyValue(vm => vm.IsCollapsed), this.WhenAnyObservable(vm => vm.Inputs.Changed), (a, b) => Unit.Default)
                .Select(_ => IsCollapsed ? (IList<NodeInputViewModel>)Inputs.Where(i => i.Connection != null).ToList() : Inputs)
                .BindListContents(this, vm => vm.VisibleInputs);

            Observable.CombineLatest(this.WhenAnyValue(vm => vm.IsCollapsed), this.WhenAnyObservable(vm => vm.Outputs.Changed), (a, b) => Unit.Default)
                .Select(_ => IsCollapsed ? (IList<NodeOutputViewModel>)Outputs.Where(o => o.Connections.Count != 0).ToList() : Outputs)
                .BindListContents(this, vm => vm.VisibleOutputs);
            
            this.CanBeRemovedByUser = true;
        }
    }
}
