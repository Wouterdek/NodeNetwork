﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DynamicData;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using NodeNetwork.Views.Controls;
using ReactiveUI;

namespace NodeNetwork.Views
{
    public partial class NetworkView : IViewFor<NetworkViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NetworkViewModel), typeof(NetworkView), new PropertyMetadata(null));

        public NetworkViewModel ViewModel
        {
            get => (NetworkViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NetworkViewModel)value;
        }
        #endregion

        #region NetworkViewportRegion
        /// <summary>
        /// The rectangle to use as a clipping mask for contentContainer
        /// </summary>
        public Rect NetworkViewportRegion
        {
            get
            {
                double left = Canvas.GetLeft(contentContainer);
                if (Double.IsNaN(left))
                {
                    left = 0;
                }

                double top = Canvas.GetTop(contentContainer);
                if (Double.IsNaN(top))
                {
                    top = 0;
                }

                if (contentContainer.RenderTransform is ScaleTransform)
                {
                    GeneralTransform transform = this.TransformToDescendant(contentContainer);
                    return transform.TransformBounds(new Rect(0, 0, this.ActualWidth, this.ActualHeight));
                }
                return new Rect(-left, -top, this.ActualWidth, this.ActualHeight);
            }
        }
        private BindingExpressionBase _viewportBinding;
		#endregion

		/// <summary>
		/// The element that is used as an origin for the position of the elements of the network.
		/// </summary>
		/// <example>
		/// Can be used for calculating the mouse position relative to the network.
		/// <code>
		/// Mouse.GetPosition(network.CanvasOriginElement)
		/// </code>
		/// </example>
		public IInputElement CanvasOriginElement => contentContainer;

		public NetworkView()
        {
            InitializeComponent();
	        if (DesignerProperties.GetIsInDesignMode(this)) { return; }

			SetupNodes();
            SetupConnections();
            SetupCutLine();
            SetupViewportBinding();
            SetupKeyboardShortcuts();
            SetupErrorMessages();
            SetupDragAndDrop();
            SetupSelectionRectangle();
        }

        #region Setup
        private void SetupNodes()
        {
            this.WhenActivated(d => d(
                this.OneWayBind(ViewModel, vm => vm.Nodes, v => v.nodesControl.ItemsSource)
            ));
        }

        private void SetupConnections()
        {
            this.WhenActivated(d =>
            {
	            this.BindList(ViewModel, vm => vm.Connections, v => v.connectionsControl.ItemsSource).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.Connections, v => v.connectionsControl.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PendingConnection, v => v.pendingConnectionView.ViewModel).DisposeWith(d);

                this.Events().MouseMove
                    .Select(e => e.GetPosition(contentContainer))
                    .BindTo(this, v => v.ViewModel.PendingConnection.LooseEndPoint)
                    .DisposeWith(d);

                this.Events().MouseLeftButtonUp
                    .Where(_ => ViewModel.PendingConnection != null)
                    .Subscribe(_ => ViewModel.RemovePendingConnection())
                    .DisposeWith(d);
            });
        }

        private void SetupKeyboardShortcuts()
        {
            this.WhenActivated(d =>
            {
                this.Events().MouseLeftButtonDown.Subscribe(_ => Focus()).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DeleteSelectedNodes, v => v.deleteBinding.Command).DisposeWith(d);
            });
        }

        private void SetupCutLine()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.CutLine.StartPoint.X, v => v.cutLine.X1).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CutLine.StartPoint.Y, v => v.cutLine.Y1).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CutLine.EndPoint.X, v => v.cutLine.X2).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CutLine.EndPoint.Y, v => v.cutLine.Y2).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CutLine.IsVisible, v => v.cutLine.Visibility,
                    isVisible => isVisible ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(d);

                dragCanvas.Events().MouseRightButtonDown.Subscribe(e =>
                {
                    Point pos = e.GetPosition(contentContainer);
                    ViewModel.CutLine.StartPoint = pos;
                    ViewModel.CutLine.EndPoint = pos;

                    e.Handled = true;
                }).DisposeWith(d);

                dragCanvas.Events().MouseMove.Subscribe(e =>
                {
	                if (e.RightButton == MouseButtonState.Pressed)
	                {
		                if (!ViewModel.CutLine.IsVisible)
		                {
			                ViewModel.StartCut();
						}

						ViewModel.CutLine.EndPoint = e.GetPosition(contentContainer);

		                ViewModel.CutLine.IntersectingConnections.Edit(l =>
		                {
			                l.Clear();
			                l.AddRange(FindIntersectingConnections().Where(val => val.intersects).Select(val => val.con));
						});

		                e.Handled = true;
					}
                    
                }).DisposeWith(d);

                dragCanvas.Events().MouseRightButtonUp.Subscribe(e =>
                {
	                if (ViewModel.CutLine.IsVisible)
	                {
		                //Do cuts
		                ViewModel.FinishCut();

		                e.Handled = true;
	                }
                }).DisposeWith(d);
            });
        }

        private void SetupViewportBinding()
        {
            Binding binding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(NetworkViewportRegion)),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _viewportBinding = BindingOperations.SetBinding(clippingGeometry, RectangleGeometry.RectProperty, binding);
        }

        private void SetupErrorMessages()
        {
            messageHostBorder.Visibility = Visibility.Collapsed; //Start collapsed
            messagePopup.VerticalOffset = -15;

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.LatestValidation.IsValid, v => v.messageHostBorder.Visibility,
                    isValid => isValid ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.LatestValidation.MessageViewModel, v => v.messageHost.ViewModel)
                    .DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.PendingConnection.Validation)
                    .Select(_ => ViewModel.PendingConnection?.Validation?.MessageViewModel != null)
                    .BindTo(this, v => v.messagePopup.IsOpen)
                    .DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.PendingConnection.Validation)
                    .Select(_ => ViewModel.PendingConnection?.Validation?.MessageViewModel)
                    .BindTo(this, v => v.messagePopupHost.ViewModel)
                    .DisposeWith(d);

                this.WhenAnyValue(vm => vm.ViewModel.PendingConnection.BoundingBox)
                    .Select(b => new Rect(contentContainer.TranslatePoint(b.TopLeft, this), contentContainer.TranslatePoint(b.BottomRight, this)))
                    .BindTo(this, v => v.messagePopup.PlacementRectangle)
                    .DisposeWith(d);
                this.WhenAnyValue(vm => vm.ViewModel.PendingConnection.BoundingBox)
                    .Select(b => (b.Width / 2d) - (messagePopup.Child.RenderSize.Width / 2d))
                    .BindTo(this, v => v.messagePopup.HorizontalOffset)
                    .DisposeWith(d);
            });
        }

        private void SetupDragAndDrop()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PendingNode, v => v.pendingNodeView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PendingNode, v => v.pendingNodeView.Visibility,
                    node => node == null ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.PendingNode.Position).Subscribe(pos =>
                {
                    Canvas.SetLeft(pendingNodeView, pos.X);
                    Canvas.SetTop(pendingNodeView, pos.Y);
                }).DisposeWith(d);

                this.Events().DragOver.Subscribe(e =>
                {
                    object data = e.Data.GetData("nodeVM");
                    NodeViewModel newNodeVm = data as NodeViewModel;

                    ViewModel.PendingNode = newNodeVm;
                    if (ViewModel.PendingNode != null)
                    {
                        ViewModel.PendingNode.Position = e.GetPosition(contentContainer);
                    }

                    e.Effects = newNodeVm != null ? DragDropEffects.Copy : DragDropEffects.None;
                }).DisposeWith(d);

                this.Events().Drop.Subscribe(e =>
                {
                    object data = e.Data.GetData("nodeVM");
                    NodeViewModel newNodeVm = data as NodeViewModel;
                    if (newNodeVm != null)
                    {
                        this.ViewModel.PendingNode =
                            new NodeViewModel(); //Fixes issue with newNodeVm sticking around in pendingNodeView, messing up position updates
                        this.ViewModel.PendingNode = null;
                        newNodeVm.Position = e.GetPosition(contentContainer);
                        this.ViewModel.Nodes.Add(newNodeVm);
                    }
                }).DisposeWith(d);

                this.Events().DragLeave.Subscribe(_ => ViewModel.PendingNode = null).DisposeWith(d);
            });
        }

        private void SetupSelectionRectangle()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(vm => vm.ViewModel.SelectionRectangle.Rectangle.Left)
                    .Subscribe(left => Canvas.SetLeft(selectionRectangle, left))
                    .DisposeWith(d);
                this.WhenAnyValue(vm => vm.ViewModel.SelectionRectangle.Rectangle.Top)
                    .Subscribe(top => Canvas.SetTop(selectionRectangle, top))
                    .DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectionRectangle.Rectangle.Width, v => v.selectionRectangle.Width).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectionRectangle.Rectangle.Height, v => v.selectionRectangle.Height).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectionRectangle.IsVisible, v => v.selectionRectangle.Visibility).DisposeWith(d);

                this.Events().PreviewMouseDown.Subscribe(e =>
                {
                    if (ViewModel != null && e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        CaptureMouse();
                        dragCanvas.IsDraggingEnabled = false;
                        ViewModel.StartRectangleSelection();
                        ViewModel.SelectionRectangle.StartPoint = e.GetPosition(contentContainer);
                        ViewModel.SelectionRectangle.EndPoint = ViewModel.SelectionRectangle.StartPoint;
                    }
                }).DisposeWith(d);

                this.Events().MouseMove.Subscribe(e =>
                {
                    if (ViewModel != null && ViewModel.SelectionRectangle.IsVisible)
                    {
                        ViewModel.SelectionRectangle.EndPoint = e.GetPosition(contentContainer);
                        UpdateSelectionRectangleIntersections();
                    }
                }).DisposeWith(d);

                this.Events().MouseUp.Subscribe(e =>
                {
                    if (ViewModel != null && ViewModel.SelectionRectangle.IsVisible)
                    {
                        ViewModel.FinishRectangleSelection();
                        dragCanvas.IsDraggingEnabled = true;
                        ReleaseMouseCapture();
                    }
                }).DisposeWith(d);
            });
        }

        // Real, accurate but expensive hittesting
        /*private void UpdateSelectionRectangleIntersections()
        {
            RectangleGeometry geometry = new RectangleGeometry(ViewModel.SelectionRectangle.Rectangle);

            ViewModel.SelectionRectangle.IntersectingNodes.Clear();
            VisualTreeHelper.HitTest(nodesControl, element =>
            {
                if (element is NodeView)
                {
                    //return HitTestFilterBehavior.ContinueSkipChildren;
                }

                return HitTestFilterBehavior.Continue;
            }, result =>
            {
                if ((result.VisualHit as FrameworkElement)?.DataContext is NodeViewModel nodeVm &&
                    !ViewModel.SelectionRectangle.IntersectingNodes.Contains(nodeVm))
                {
                    Debug.WriteLine(result.VisualHit);
                    ViewModel.SelectionRectangle.IntersectingNodes.Add(nodeVm);
                }

                return HitTestResultBehavior.Continue;
            }, new GeometryHitTestParameters(geometry));
        }*/

        // Approximate but cheap boundingbox-based hittesting
        private void UpdateSelectionRectangleIntersections()
        {
            var selectionRect = ViewModel.SelectionRectangle.Rectangle;

            var nodesHit = WPFUtils.FindDescendantsOfType<NodeView>(nodesControl, true)
                    .Where(nodeView =>
                    {
                        //return selectionRect.Contains(new Rect(nodeView.ViewModel.Position, nodeView.RenderSize));
                        return selectionRect.IntersectsWith(new Rect(nodeView.ViewModel.Position, nodeView.RenderSize));
                    })
                .Select(view => view.ViewModel);

            ViewModel.SelectionRectangle.IntersectingNodes.Clear();
            ViewModel.SelectionRectangle.IntersectingNodes.AddRange(nodesHit);
        }
        #endregion

        #region Viewport bound updates
        private void DragCanvas_OnZoom(object source, ZoomEventArgs args)
        {
            _viewportBinding?.UpdateTarget();
        }

        private void ContentContainer_OnLayoutUpdated(object sender, EventArgs e)
        {
            _viewportBinding?.UpdateTarget();
        }
        #endregion

        private void OnDragNode(object sender, DragDeltaEventArgs e)
        {
            foreach (NodeViewModel node in ViewModel.SelectedNodes)
            {
                node.Position = new Point(node.Position.X + e.HorizontalChange, node.Position.Y + e.VerticalChange);
            }
        }

        private void OnClickCanvas(object sender, MouseButtonEventArgs e)
        {
            ViewModel.ClearSelection();
        }

        private IEnumerable<(ConnectionViewModel con, bool intersects)> FindIntersectingConnections()
        {
            foreach (ConnectionViewModel con in ViewModel.Connections.Items)
            {
                PathGeometry conGeom = ConnectionView.BuildSmoothBezier(con.Input.Port.CenterPoint, con.Output.Port.CenterPoint);
                LineGeometry cutLineGeom = new LineGeometry(ViewModel.CutLine.StartPoint, ViewModel.CutLine.EndPoint);
                bool hasIntersections = WPFUtils.GetIntersectionPoints(conGeom, cutLineGeom).Any();
                yield return (con, hasIntersections);
            }
        }
    }
}
