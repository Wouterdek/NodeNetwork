using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Represents a connection between a node input and a node output
    /// </summary>
    [DataContract]
    public class ConnectionViewModel : ReactiveObject
    {
        static ConnectionViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new ConnectionView(), typeof(IViewFor<ConnectionViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The network that contains this connection
        /// </summary>
        [DataMember]
        public NetworkViewModel Parent { get; }
        #endregion

        #region Input
        /// <summary>
        /// The viewmodel of the node input that is on one end of the connection.
        /// </summary>
        [DataMember]
        public NodeInputViewModel Input { get; }
        #endregion

        #region Output
        /// <summary>
        /// The viewmodel of the node output that is on one end of the connection.
        /// </summary>
        [DataMember]
        public NodeOutputViewModel Output { get; } 
        #endregion

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
        private bool _canBeRemovedByUser;
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
        private bool _isHighlighted;
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
        private bool _isInErrorState;
        #endregion

        #region IsMarkedForDelete
        /// <summary>
        /// If true, the connection is displayed as being marked for deletion.
        /// </summary>
        [IgnoreDataMember]
        public bool IsMarkedForDelete => _isMarkedForDelete.Value;
        private ObservableAsPropertyHelper<bool> _isMarkedForDelete;
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
