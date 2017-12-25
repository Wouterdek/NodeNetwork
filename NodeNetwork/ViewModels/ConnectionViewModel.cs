using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class ConnectionViewModel : ReactiveObject
    {
        static ConnectionViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new ConnectionView(), typeof(IViewFor<ConnectionViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public NetworkViewModel Parent { get; }

        public NodeInputViewModel Input { get; }
        public NodeOutputViewModel Output { get; }
        
        #region CanBeRemovedByUser
        private bool _canBeRemovedByUser;
        public bool CanBeRemovedByUser
        {
            get => _canBeRemovedByUser;
            set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
        }
        #endregion
        
        #region IsHighlighted
        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => this.RaiseAndSetIfChanged(ref _isHighlighted, value);
        }
        #endregion
        
        #region IsInErrorState
        private bool _isInErrorState;
        public bool IsInErrorState
        {
            get => _isInErrorState;
            set => this.RaiseAndSetIfChanged(ref _isInErrorState, value);
        }
        #endregion
        
        #region IsMarkedForDelete
        private ObservableAsPropertyHelper<bool> _isMarkedForDelete;
        public bool IsMarkedForDelete => _isMarkedForDelete.Value;
        #endregion

        public ConnectionViewModel(NetworkViewModel parent, NodeInputViewModel input, NodeOutputViewModel output)
        {
            Parent = parent;
            Input = input;
            Output = output;

            this.WhenAnyObservable(v => v.Parent.CutLine.IntersectingConnections.Changed)
                .Where(_ => Parent?.CutLine != null)
                .Select(_ => Parent.CutLine.IntersectingConnections.Contains(this))
                .ToProperty(this, vm => vm.IsMarkedForDelete, out _isMarkedForDelete);
        }
    }
}
