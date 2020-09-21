using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for the UI part of an endpoint that is used to create connections.
    /// </summary>
    public class PortViewModel : ReactiveObject
    {
        static PortViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new PortView(), typeof(IViewFor<PortViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The Endpoint that owns this port.
        /// </summary>
        public Endpoint Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        private Endpoint _parent;
        #endregion

        #region CenterPoint
        /// <summary>
        /// The coordinates, relative to the network, of the center of this port.
        /// Used to draw connections.
        /// </summary>
        public Point CenterPoint
        {
            get => _centerPoint;
            set => this.RaiseAndSetIfChanged(ref _centerPoint, value);
        }
        private Point _centerPoint;
        #endregion

        #region IsMirrored
        /// <summary>
        /// If true, the view for this viewmodel will be horizontally mirrored.
        /// </summary>
        public bool IsMirrored
        {
            get => _isMirrored;
            set => this.RaiseAndSetIfChanged(ref _isMirrored, value);
        }
        private bool _isMirrored;
        #endregion

        #region IsVisible
        /// <summary>
        /// If true, this port is visible. If false, this port is hidden.
        /// True by default.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
        private bool _isVisible;
        #endregion

        #region IsHighlighted
        /// <summary>
        /// If true, this port is highlighted.
        /// This could be, for example, because the mouse is hovering over the port.
        /// </summary>
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => this.RaiseAndSetIfChanged(ref _isHighlighted, value);
        }
        private bool _isHighlighted;
        #endregion

        #region IsInErrorMode
        /// <summary>
        /// If true, the port will visually indicate there is an error with this port.
        /// In the default view this is used to indicate a pending connection validation error.
        /// </summary>
        public bool IsInErrorMode
        {
            get => _isInErrorMode;
            set => this.RaiseAndSetIfChanged(ref _isInErrorMode, value);
        }
        private bool _isInErrorMode;
        #endregion

        #region ConnectionDragStarted
        /// <summary>
        /// Observable that fires when the user starts a new pending connection from this port.
        /// </summary>
        public IObservable<Unit> ConnectionDragStarted => _connectionDragStarted;
        private readonly Subject<Unit> _connectionDragStarted = new Subject<Unit>();
        #endregion

        #region ConnectionPreview
        /// <summary>
        /// Fires when a pending connection is dragged over this port.
        /// </summary>
        public IObservable<bool> ConnectionPreviewActive => _connectionPreviewActive;
        private readonly Subject<bool> _connectionPreviewActive = new Subject<bool>();
        #endregion

        #region ConnectionDragFinished
        /// <summary>
        /// Fires when the user drops the pending connection on this port.
        /// </summary>
        public IObservable<Unit> ConnectionDragFinished => _connectionDragFinished;
        private readonly Subject<Unit> _connectionDragFinished = new Subject<Unit>();
        #endregion

        public PortViewModel()
        {
            IsVisible = true;
        }

        public void OnDragFromPort()
        {
            _connectionDragStarted.OnNext(Unit.Default);
        }

        public void OnPortEnter()
        {
            IsHighlighted = true;

            PendingConnectionViewModel pendingConnection = Parent.Parent?.Parent?.PendingConnection;
            if (pendingConnection != null && pendingConnection.Input != Parent && pendingConnection.Output != Parent)
            {
                _connectionPreviewActive.OnNext(true);
            }
        }

        public void OnPortLeave()
        {
            IsHighlighted = false;

            PendingConnectionViewModel pendingConnection = Parent.Parent?.Parent?.PendingConnection;
            if (pendingConnection != null)
            {
                _connectionPreviewActive.OnNext(false);
            }
        }

        public void OnDropOnPort()
        {
            if (Parent?.Parent?.Parent?.PendingConnection != null)
            {
                _connectionDragFinished.OnNext(Unit.Default);
            }
        }
    }
}
