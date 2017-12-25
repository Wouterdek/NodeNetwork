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
    public class PortViewModel : ReactiveObject
    {
        static PortViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new PortView(), typeof(IViewFor<PortViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        private IEndpoint _parent;
        public IEndpoint Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        #endregion
        
        #region CenterPoint
        private Point _centerPoint;
        public Point CenterPoint
        {
            get => _centerPoint;
            set => this.RaiseAndSetIfChanged(ref _centerPoint, value);
        }
        #endregion
        
        #region IsVisible
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
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

        #region IsInErrorMode
        private bool _isInErrorMode;
        public bool IsInErrorMode
        {
            get => _isInErrorMode;
            set => this.RaiseAndSetIfChanged(ref _isInErrorMode, value);
        }
        #endregion

        #region ConnectionDragStarted
        private readonly Subject<Unit> _connectionDragStarted = new Subject<Unit>();
        public IObservable<Unit> ConnectionDragStarted => _connectionDragStarted;
        #endregion

        #region ConnectionPreview
        private readonly Subject<bool> _connectionPreviewActive = new Subject<bool>();
        public IObservable<bool> ConnectionPreviewActive => _connectionPreviewActive;
        #endregion

        #region ConnectionDragFinished
        private readonly Subject<Unit> _connectionDragFinished = new Subject<Unit>();
        public IObservable<Unit> ConnectionDragFinished => _connectionDragFinished;
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
