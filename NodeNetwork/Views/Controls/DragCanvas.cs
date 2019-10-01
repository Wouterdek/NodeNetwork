using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeNetwork.Views.Controls
{
    public class DragCanvas : Canvas
    {
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

        /// <summary> 
        /// This event puts the control into a state where it is ready for a drag operation.
        /// </summary>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsDraggingEnabled)
            {
                _userClickedThisElement = true;

	            _previousMouseScreenPos = _originScreenCoordPosition = e.GetPosition(this);
				Focus();
                CaptureMouse(); //All mouse events will now be handled by the dragcanvas
            }

            base.OnMouseLeftButtonDown(e);
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

                    ApplyDragToChildren(dragEvent);

                    _previousMouseScreenPos = curMouseScreenPos;
                }
            }

            base.OnMouseMove(e);
        }


        /// <summary>
        /// Stop dragging when the user releases the left mouse button
        /// </summary>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
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

            base.OnMouseLeftButtonUp(e);
        }

        private void ApplyDragToChildren(DragMoveEventArgs drag)
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

                Canvas.SetLeft(cur, prevLeft + (drag.DeltaX));
                Canvas.SetTop(cur, prevTop + (drag.DeltaY));
            }
        }
        #endregion

        #region Zoom
        public delegate void ZoomEvent(object source, ZoomEventArgs args);
        public event ZoomEvent Zoom;

        private int _wheelOffset = 6;
        private const int MinWheelOffset = 1;
        private const int MaxWheelOffset = 15;
        
        private ScaleTransform _curScaleTransform = new ScaleTransform(1.0, 1.0);

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
            if ((_wheelOffset == MinWheelOffset && e.Delta < 0) || (_wheelOffset == MaxWheelOffset && e.Delta > 0))
            {
                return;
            }

            _wheelOffset += e.Delta / 120;
            if (_wheelOffset < MinWheelOffset)
            {
                _wheelOffset = MinWheelOffset;
            }
            else if (_wheelOffset > MaxWheelOffset)
            {
                _wheelOffset = MaxWheelOffset;
            }

            double oldScale = _curScaleTransform.ScaleX;
            double newScale = Math.Log(1 + ((_wheelOffset) / 10d)) * 2d;

            //Calculate current viewing window onto the content
            Point topLeftContentSpace = TranslatePoint(new Point(0, 0), Children[0]);
            Point bottomRightContentSpace = TranslatePoint(new Point(ActualWidth, ActualHeight), Children[0]);
            Rect curView = new Rect
            {
                Location = topLeftContentSpace,
                Size = new Size(bottomRightContentSpace.X - topLeftContentSpace.X, bottomRightContentSpace.Y - topLeftContentSpace.Y)
            };

            //Mouse position as a fraction of the view size
            Point viewSpaceMousePos = e.GetPosition(this);
            Point relZoomPoint = new Point
            {
                X = viewSpaceMousePos.X / this.ActualWidth,
                Y = viewSpaceMousePos.Y / this.ActualHeight
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

            var zoomEvent = new ZoomEventArgs(e, _curScaleTransform, newScaleTransform, newOffset);
            Zoom?.Invoke(this, zoomEvent);

            ApplyZoomToChildren(zoomEvent);

            _curScaleTransform = newScaleTransform;
        }

        private void ApplyZoomToChildren(ZoomEventArgs e)
        {
            foreach (UIElement cur in this.Children)
            {
                cur.RenderTransform = e.NewScale;
                Canvas.SetLeft(cur, e.ContentOffset.X);
                Canvas.SetTop(cur, e.ContentOffset.Y);
            }
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
