using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel for the view that is used to select nodes by dragging a rectangle around them.
    /// </summary>
    public class SelectionRectangleViewModel : ReactiveObject
    {
        #region StartPoint
        /// <summary>
        /// The coordinates of the first corner of the rectangle (where the user clicked down).
        /// </summary>
        public Point StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }
        private Point _startPoint;
        #endregion

        #region EndPoint
        /// <summary>
        /// The coordinates of the second corner of the rectangle.
        /// </summary>
        public Point EndPoint
        {
            get => _endPoint;
            set => this.RaiseAndSetIfChanged(ref _endPoint, value);
        }
        private Point _endPoint;
        #endregion

        #region Rectangle
        /// <summary>
        /// The Rect object formed by StartPoint and EndPoint.
        /// </summary>
        public Rect Rectangle => _rectangle.Value;
        private readonly ObservableAsPropertyHelper<Rect> _rectangle;
        #endregion

        #region IsVisible
        /// <summary>
        /// If true, the selection rectangle view is visible.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
        private bool _isVisible;
        #endregion

        #region IntersectingNodes
        /// <summary>
        /// List of nodes visually intersecting or contained in the rectangle.
        /// This list is driven by the view.
        /// </summary>
        public ISourceList<NodeViewModel> IntersectingNodes { get; } = new SourceList<NodeViewModel>();
        #endregion

        public SelectionRectangleViewModel()
        {
            this.WhenAnyValue(vm => vm.StartPoint, vm => vm.EndPoint)
                .Select(_ => new Rect(StartPoint, EndPoint))
                .ToProperty(this, vm => vm.Rectangle, out _rectangle);
			
			IntersectingNodes.Connect().ActOnEveryObject(node => node.IsSelected = true, node => node.IsSelected = false);
        }
    }
}
