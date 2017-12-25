using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class SelectionRectangleViewModel : ReactiveObject
    {
        #region StartPoint
        private Point _startPoint;
        public Point StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }
        #endregion

        #region EndPoint
        private Point _endPoint;
        public Point EndPoint
        {
            get => _endPoint;
            set => this.RaiseAndSetIfChanged(ref _endPoint, value);
        }
        #endregion

        #region Rectangle
        private readonly ObservableAsPropertyHelper<Rect> _rectangle;
        public Rect Rectangle => _rectangle.Value; 
        #endregion

        #region IsVisible
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
        #endregion

        #region IntersectingNodes
        public ReactiveList<NodeViewModel> IntersectingNodes { get; } = new ReactiveList<NodeViewModel>();
        #endregion

        public SelectionRectangleViewModel()
        {
            this.WhenAnyValue(vm => vm.StartPoint, vm => vm.EndPoint)
                .Select(_ => new Rect(StartPoint, EndPoint))
                .ToProperty(this, vm => vm.Rectangle, out _rectangle);

            //Note: ActOnEveryObject does not work properly with SuppressChangeNotifications
            IntersectingNodes.ActOnEveryObject(node => node.IsSelected = true, node => node.IsSelected = false);
        }
    }
}
