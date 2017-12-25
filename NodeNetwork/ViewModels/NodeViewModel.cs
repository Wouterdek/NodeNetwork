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
        private NetworkViewModel _parent;
        public NetworkViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        #endregion
        
        #region Name
        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        #endregion
        
        #region Inputs
        private readonly ReactiveList<NodeInputViewModel> _inputs = new ReactiveList<NodeInputViewModel> {ChangeTrackingEnabled = true};
        public IReactiveList<NodeInputViewModel> Inputs => _inputs;
        #endregion

        #region Outputs
        private readonly ReactiveList<NodeOutputViewModel> _outputs = new ReactiveList<NodeOutputViewModel> {ChangeTrackingEnabled = true};
        public IReactiveList<NodeOutputViewModel> Outputs => _outputs;
        #endregion

        #region VisibleInputs
        public IReactiveList<NodeInputViewModel> VisibleInputs { get; } = new ReactiveList<NodeInputViewModel>();
        #endregion

        #region VisibleOutputs
        public IReactiveList<NodeOutputViewModel> VisibleOutputs { get; } = new ReactiveList<NodeOutputViewModel>();
        #endregion

        #region ToolTip
        private object _toolTip;
        public object ToolTip
        {
            get => _toolTip;
            set => this.RaiseAndSetIfChanged(ref _toolTip, value);
        }
        #endregion
        
        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        #endregion
        
        #region IsCollapsed
        private bool _isCollapsed;
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => this.RaiseAndSetIfChanged(ref _isCollapsed, value);
        }
        #endregion

        #region CanBeRemovedByUser
        private bool _canBeRemovedByUser;
        public bool CanBeRemovedByUser
        {
            get => _canBeRemovedByUser;
            set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
        }
        #endregion
        
        #region Position
        private Point _position;
        public Point Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }
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
