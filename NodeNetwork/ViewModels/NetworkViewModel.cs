using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;

namespace NodeNetwork.ViewModels
{
    public class NetworkViewModel : ReactiveObject
    {
        static NetworkViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NetworkView(), typeof(IViewFor<NetworkViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Nodes
        private readonly ReactiveList<NodeViewModel> _nodes = new ReactiveList<NodeViewModel> {ChangeTrackingEnabled = true};
        public IReactiveList<NodeViewModel> Nodes => _nodes;
        #endregion

        #region SelectedNodes
        public IReactiveDerivedList<NodeViewModel> SelectedNodes { get; }
        #endregion

        #region Connections
        public IReactiveList<ConnectionViewModel> Connections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion

        #region PendingConnection 
        private PendingConnectionViewModel _pendingConnection;
        public PendingConnectionViewModel PendingConnection
        {
            get => _pendingConnection;
            set => this.RaiseAndSetIfChanged(ref _pendingConnection, value);
        }
        #endregion

        #region PendingNode
        private NodeViewModel _pendingNode;
        public NodeViewModel PendingNode
        {
            get => _pendingNode;
            set => this.RaiseAndSetIfChanged(ref _pendingNode, value);
        }
        #endregion

        #region ConnectionFactory
        private Func<NodeInputViewModel, NodeOutputViewModel, ConnectionViewModel> _connectionFactory;
        public Func<NodeInputViewModel, NodeOutputViewModel, ConnectionViewModel> ConnectionFactory
        {
            get => _connectionFactory;
            set => this.RaiseAndSetIfChanged(ref _connectionFactory, value);
        }
        #endregion
        
        #region Validator
        private Func<NetworkViewModel, NetworkValidationResult> _validator;
        public Func<NetworkViewModel, NetworkValidationResult> Validator
        {
            get => _validator;
            set => this.RaiseAndSetIfChanged(ref _validator, value);
        }
        #endregion

        #region LatestValidation
        //Using ObservableAsPropertyHelper would be better, but causes problems with ReactiveCommand where
        //the value of the property is updated only after the subscribers to the command are run.
        private NetworkValidationResult _latestValidation;
        public NetworkValidationResult LatestValidation
        {
            get => _latestValidation;
            private set => this.RaiseAndSetIfChanged(ref _latestValidation, value);
        }
        //private readonly ObservableAsPropertyHelper<NetworkValidationResult> _latestValidation;
        //public NetworkValidationResult LatestValidation => _latestValidation.Value;
        #endregion

        public IObservable<NetworkValidationResult> Validation { get; }
        
        #region IsReadOnly
        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }
        #endregion
        
        #region CutLine
        public CutLineViewModel CutLine { get; } = new CutLineViewModel();
        #endregion
        
        #region SelectionRectangle
        public SelectionRectangleViewModel SelectionRectangle { get; } = new SelectionRectangleViewModel();
        #endregion

        #region Commands
        public ReactiveCommand DeleteSelectedNodes { get; }
        public ReactiveCommand<Unit, NetworkValidationResult> UpdateValidation { get; }
        #endregion

        public NetworkViewModel()
        {
            Nodes.BeforeItemsAdded.Subscribe(node => node.Parent = this);
            Nodes.BeforeItemsRemoved.Subscribe(node => node.Parent = null);
            
            SelectedNodes = Nodes.CreateDerivedCollection(node => node, node => node.IsSelected);

            DeleteSelectedNodes = ReactiveCommand.Create(() =>
            {
                var nodesToRemove = SelectedNodes.Where(n => n.CanBeRemovedByUser).ToArray();
                foreach (NodeViewModel node in nodesToRemove)
                {
                    Connections.RemoveAll(node.Inputs.Select(i => i.Connection).Where(c => c != null).ToArray());
                    Connections.RemoveAll(node.Outputs.SelectMany(o => o.Connections).ToArray());
                }

                bool pendingConnectionInvalid = new[] {PendingConnection?.Input?.Parent, PendingConnection?.Output?.Parent}.Any(n => nodesToRemove.Contains(n));
                if (pendingConnectionInvalid)
                {
                    RemovePendingConnection();
                }

                Nodes.RemoveAll(nodesToRemove);
            });

            ConnectionFactory = (input, output) => new ConnectionViewModel(this, input, output);

            Validator = _ => new NetworkValidationResult(true, true, null);
            UpdateValidation = ReactiveCommand.Create(() => {
                var result = Validator(this);
                LatestValidation = result;
                return result;
            });
            //UpdateValidation.ToProperty(this, vm => vm.LatestValidation, out _latestValidation);

            var onValidationPropertyUpdate = this.WhenAnyValue(vm => vm.LatestValidation).Multicast(new Subject<NetworkValidationResult>());
            onValidationPropertyUpdate.Connect();
            Validation = Observable.Create<NetworkValidationResult>(obs =>
            {
                obs.OnNext(LatestValidation);
                obs.OnCompleted();
                return Disposable.Empty;
            }).Concat(onValidationPropertyUpdate);
            
            Connections.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation);
            Nodes.Changed.Select(_ => Unit.Default).InvokeCommand(UpdateValidation);
        }
        
        public void ClearSelection()
        {
            using (SelectedNodes.SuppressChangeNotifications())
            {
                foreach (NodeViewModel node in SelectedNodes.ToArray())
                {
                    node.IsSelected = false;
                }
            }
        }

        public void StartCut()
        {
            CutLine.IsVisible = true;
        }

        public void FinishCut()
        {
            Connections.RemoveAll(CutLine.IntersectingConnections);
            CutLine.IntersectingConnections.Clear();
            CutLine.IsVisible = false;
        }

        public void RemovePendingConnection()
        {
            PendingConnection = null;
        }

        public void StartRectangleSelection()
        {
            ClearSelection();
            SelectionRectangle.IsVisible = true;
            SelectionRectangle.IntersectingNodes.Clear();
        }

        public void FinishRectangleSelection()
        {
            SelectionRectangle.IsVisible = false;
        }
    }
}
