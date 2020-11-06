using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeNetwork.Views.Controls
{
    public class DragCanvas : Canvas
    {
        #region Position
        public static readonly DependencyProperty DragOffsetProperty = DependencyProperty.Register(nameof(DragOffset),
            typeof(Point), typeof(DragCanvas), new PropertyMetadata(new Point(), DragOffsetChanged));

        /// <summary>
        /// Gets or sets the current canvas drag offset.
        /// </summary>
        public Point DragOffset
        {
            get => (Point)GetValue(DragOffsetProperty);
            set => SetValue(DragOffsetProperty, value);
        }

        private static void DragOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (DragCanvas)d;
            if (e.NewValue is Point position)
            {
                canvas.ApplyDragToChildren(position.X - canvas._previousDragOffset.X, position.Y - canvas._previousDragOffset.Y);
            }
        }
        #endregion

        #region Dragging

        /// <summary>
        /// Triggered when the user clicks and moves the canvas, starting a drag
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">The mouseevent that triggered this event</param>
        public delegate void DragStartEventHandler(object sender, MouseEventArgs args);
        public event DragStartEventHandler DragStart;

        /// <summary>
        /// Triggered when the user drags the canvas
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">Contains the distance traveled since the last drag move or drag start event</param>
        public delegate void DragMoveEventHandler(object sender, DragMoveEventArgs args);
        public event DragMoveEventHandler DragMove;

        /// <summary>
        /// Triggered when the user releases the mouse and the drag stops.
        /// </summary>
        /// <param name="sender">The dragcanvas that triggered this event</param>
        /// <param name="args">Contains the total distance traveled</param>
        public delegate void DragEndEventHandler(object sender, DragMoveEventArgs args);
        public event DragEndEventHandler DragStop;

        public bool IsDraggingEnabled { get; set; } = true;

        #region StartDragGesture
        public static readonly DependencyProperty StartDragGestureProperty = DependencyProperty.Register(nameof(StartDragGesture),
            typeof(MouseGesture), typeof(DragCanvas), new PropertyMetadata(new MouseGesture(MouseAction.LeftClick)));

        /// <summary>
        /// This mouse gesture starts a drag on the canvas. Left click by default.
        /// </summary>
        public MouseGesture StartDragGesture
        {
            get => (MouseGesture)GetValue(StartDragGestureProperty);
            set => SetValue(StartDragGestureProperty, value);
        }
        #endregion

        /// <summary>
        /// Used when the mousebutton is down to check if the initial click was in this element.
        /// This is useful because we dont want to assume a drag operation when the user moves the mouse but originally clicked a different element
        /// </summary>
        private bool _userClickedThisElement;

        /// <summary>
        /// Is a drag operation currently in progress?
        /// </summary>
        private bool _dragActive;

        /// <summary>
        /// The position of the mouse (screen co-ordinate) where the mouse was clicked down.
        /// </summary>
        private Point _originScreenCoordPosition;

        /// <summary> 
        /// The position of the mouse (screen co-ordinate) when the previous DragDelta event was fired 
        /// </summary>
        private Point _previousMouseScreenPos;
        private Point _previousDragOffset;

        /// <summary> 
        /// This event puts the control into a state where it is ready for a drag operation.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (IsDraggingEnabled && StartDragGesture.Matches(this, e))
            {
                _userClickedThisElement = true;

                _previousMouseScreenPos = _originScreenCoordPosition = e.GetPosition(this);
                Focus();
                CaptureMouse(); //All mouse events will now be handled by the dragcanvas
            }
        }

        /// <summary> 
        /// Trigger a dragging event when the user moves the mouse while the left mouse button is pressed
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_userClickedThisElement && !_dragActive)
            {
                _dragActive = true;
                DragStart?.Invoke(this, e);
            }

            if (_dragActive)
            {
                Point curMouseScreenPos = e.GetPosition(this);

                if (!curMouseScreenPos.Equals(_previousMouseScreenPos))
                {
                    double xDelta = curMouseScreenPos.X - _previousMouseScreenPos.X;
                    double yDelta = curMouseScreenPos.Y - _previousMouseScreenPos.Y;

                    var dragEvent = new DragMoveEventArgs(e, xDelta, yDelta);
                    DragMove?.Invoke(this, dragEvent);

                    this.DragOffset = new Point(_previousDragOffset.X + xDelta, _previousDragOffset.Y + yDelta);

                    _previousMouseScreenPos = curMouseScreenPos;
                }
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Stop dragging when the user releases the mouse button
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _userClickedThisElement = false;
            ReleaseMouseCapture(); //Stop absorbing all mouse events

            if (_dragActive)
            {
                _dragActive = false;

                Point curMouseScreenPos = e.GetPosition(this);
                double xDelta = curMouseScreenPos.X - _originScreenCoordPosition.X;
                double yDelta = curMouseScreenPos.Y - _originScreenCoordPosition.Y;

                DragStop?.Invoke(this, new DragMoveEventArgs(e, xDelta, yDelta));
            }
        }

        private void ApplyDragToChildren(double deltaX, double deltaY)
        {
            foreach (UIElement cur in Children)
            {
                double prevLeft = Canvas.GetLeft(cur);
                if (Double.IsNaN(prevLeft))
                {
                    prevLeft = 0;
                }

                double prevTop = Canvas.GetTop(cur);
                if (Double.IsNaN(prevTop))
                {
                    prevTop = 0;
                }

                Canvas.SetLeft(cur, prevLeft + (deltaX));
                Canvas.SetTop(cur, prevTop + (deltaY));
            }

            _previousDragOffset = new Point(_previousDragOffset.X + deltaX, _previousDragOffset.Y + deltaY);
        }
        #endregion

        #region Zoom
        public event EventHandler<ZoomEventArgs> Zoom;

        private double _wheelOffset = 6;

        #region ZoomFactor
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(nameof(ZoomFactor),
            typeof(double), typeof(DragCanvas), new PropertyMetadata(1d, OnZoomFactorPropChanged, ZoomFactorValueCoerce));

        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }
        private bool isUpdatingZoomFactor;

        private static void OnZoomFactorPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragCanvas dc = (DragCanvas)d;
            if (!dc.isUpdatingZoomFactor)
            {
                dc.SetZoomImpl(new Point(dc.ActualWidth / 2, dc.ActualHeight / 2), (double)e.OldValue, (double)e.NewValue, null);
            }
        }

        private static object ZoomFactorValueCoerce(DependencyObject d, object baseValue)
        {
            if (baseValue is double doubleValue)
            {
                var canvas = (DragCanvas)d;
                if (doubleValue < canvas.MinZoomFactor)
                {
                    return canvas.MinZoomFactor;
                }
                else if (doubleValue > canvas.MaxZoomFactor)
                {
                    return canvas.MaxZoomFactor;
                }
            }

            return baseValue;
        }

        #endregion

        #region MaxZoomFactor
        public static readonly DependencyProperty MaxZoomFactorProperty = DependencyProperty.Register(nameof(MaxZoomFactor),
            typeof(double), typeof(DragCanvas), new FrameworkPropertyMetadata(2.5d, MaxZoomFactorChanged));

        public double MaxZoomFactor
        {
            get { return (double)GetValue(MaxZoomFactorProperty); }
            set { SetValue(MaxZoomFactorProperty, value); }
        }

        private static void MaxZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var doubleValue = (double)e.NewValue;
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue) || doubleValue <= 0)
            {
                throw new ArgumentException("MaxZoomFactor can not be NaN, Infinity or less than zero");
            }

            var canvas = (DragCanvas)d;
            var binding = BindingOperations.GetBindingExpression(canvas, ZoomFactorProperty);
            binding?.UpdateTarget();
        }
        #endregion

        #region MinZoomFactor
        public static readonly DependencyProperty MinZoomFactorProperty = DependencyProperty.Register(nameof(MinZoomFactor),
            typeof(double), typeof(DragCanvas), new FrameworkPropertyMetadata(0.15d, MinZoomFactorChanged));

        public double MinZoomFactor
        {
            get { return (double)GetValue(MinZoomFactorProperty); }
            set { SetValue(MinZoomFactorProperty, value); }
        }

        private static void MinZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var doubleValue = (double)e.NewValue;
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue) || doubleValue <= 0)
            {
                throw new ArgumentException("MinZoomFactor can not be NaN, Infinity or less than zero");
            }

            var canvas = (DragCanvas)d;
            var binding = BindingOperations.GetBindingExpression(canvas, ZoomFactorProperty);
            binding?.UpdateTarget();
        }
        #endregion

        private Rect ZoomView(Rect curView, double curZoom, double newZoom, Point relZoomPoint) //curView in content space, relZoomPoint is relative to view space
        {
            double zoomModifier = curZoom / newZoom;
            Size newSize = new Size(curView.Width * zoomModifier, curView.Height * zoomModifier);

            Point zoomCenter = new Point(curView.X + (curView.Width * relZoomPoint.X), curView.Y + (curView.Height * relZoomPoint.Y));
            double newX = zoomCenter.X - (relZoomPoint.X * newSize.Width);
            double newY = zoomCenter.Y - (relZoomPoint.Y * newSize.Height);
            Point newPos = new Point(newX, newY);

            return new Rect(newPos, newSize);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            //Calculate new scaling factor
            var wheelOffset = _wheelOffset + (e.Delta / 120);
            double newScale = Math.Log(1 + (wheelOffset / 10d)) * 2d;
            if (newScale < MinZoomFactor)
            {
                newScale = MinZoomFactor;
            }
            else if (newScale > MaxZoomFactor)
            {
                newScale = MaxZoomFactor;
            }

            Point zoomCenter = e.GetPosition(this);
            SetZoom(zoomCenter, newScale, e);
        }

        public void SetZoom(Point zoomCenter, double newScale, MouseEventArgs parentEvent)
        {
            SetZoomImpl(zoomCenter, ZoomFactor, newScale, parentEvent);
            isUpdatingZoomFactor = true;
            ZoomFactor = newScale;
            isUpdatingZoomFactor = false;
        }

        private void SetZoomImpl(Point zoomCenter, double oldScale, double newScale, MouseEventArgs parentEvent)
        {
            //Calculate current viewing window onto the content
            Point topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            Point bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            Rect curView = new Rect
            {
                Location = topLeftContentSpace,
                Size = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y)
            };

            //Mouse position as a fraction of the view size
            Point relZoomPoint = new Point
            {
                X = zoomCenter.X / this.ActualWidth,
                Y = zoomCenter.Y / this.ActualHeight
            };

            //Calculate new viewing window
            Rect newView = ZoomView(curView, oldScale, newScale, relZoomPoint);

            //Calculate new content offset based on the new view
            Point newOffset = new Point(-newView.X * newScale, -newView.Y * newScale);

            //Calculate new viewing window scale
            ScaleTransform newScaleTransform = new ScaleTransform
            {
                ScaleX = newScale,
                ScaleY = newScale
            };

            var zoomEvent = new ZoomEventArgs(parentEvent, new ScaleTransform(oldScale, oldScale), newScaleTransform, newOffset);
            Zoom?.Invoke(this, zoomEvent);

            ApplyZoomToChildren(zoomEvent);
            DragOffset = new Point(zoomEvent.ContentOffset.X, zoomEvent.ContentOffset.Y);
            _wheelOffset = 10d * Math.Pow(Math.E, newScale / 2) - 10;
        }

        private void ApplyZoomToChildren(ZoomEventArgs e)
        {
            foreach (UIElement cur in this.Children)
            {
                cur.RenderTransform = e.NewScale;
            }
        }
        #endregion

        #region Viewport
        /// <summary>
        /// Centers the canvas and sets the minimum zoom factor for the specified viewport.
        /// </summary>
        /// <param name="viewport">The desired viewport.</param>
        public void SetViewport(Rect viewport)
        {
            // Get current view size
            var topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            var bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            var curViewSize = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y);

            // Calc new scale
            var oldZoom = ZoomFactor;
            var newScaleX = oldZoom * curViewSize.Width / viewport.Width;
            var newScaleY = oldZoom * curViewSize.Height / viewport.Height;
            // Calc new zoom
            var zoom = Math.Min(newScaleX, newScaleY);
            ZoomFactor = zoom;

            this.UpdateLayout();

            var boundingCenter = new Point(viewport.TopLeft.X + viewport.Width / 2d, viewport.TopLeft.Y + viewport.Height / 2d);

            // Update current view size
            topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            curViewSize = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y);

            // Calc new position offset
            var viewOffset = new Point(boundingCenter.X - curViewSize.Width / 2d, boundingCenter.Y - curViewSize.Height / 2d);
            this.DragOffset = new Point(-viewOffset.X * ZoomFactor, -viewOffset.Y * ZoomFactor);
        }
        #endregion
    }

    public class DragMoveEventArgs : EventArgs
    {
        public MouseEventArgs MouseEvent { get; }
        public double DeltaX { get; }
        public double DeltaY { get; }

        public DragMoveEventArgs(MouseEventArgs mouseEvent, double deltaX, double deltaY)
        {
            this.MouseEvent = mouseEvent;
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
        }
    }

    public class ZoomEventArgs : EventArgs
    {
        public MouseEventArgs MouseEvent { get; }
        public ScaleTransform OldScaleScale { get; }
        public ScaleTransform NewScale { get; }
        public Point ContentOffset { get; }

        public ZoomEventArgs(MouseEventArgs e, ScaleTransform oldScale, ScaleTransform newScale, Point contentOffset)
        {
            this.MouseEvent = e;
            this.OldScaleScale = oldScale;
            this.NewScale = newScale;
            this.ContentOffset = contentOffset;
        }
    }
}
