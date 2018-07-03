using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplateVisualState(Name = HighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = NonHighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = ErrorState, GroupName = ErrorVisualStatesGroup)]
    [TemplateVisualState(Name = NonErrorState, GroupName = ErrorVisualStatesGroup)]
    [TemplateVisualState(Name = MarkedForDeleteState, GroupName = MarkedForDeleteVisualStatesGroup)]
    [TemplateVisualState(Name = NotMarkedForDeleteState, GroupName = MarkedForDeleteVisualStatesGroup)]
    public class ConnectionView : Control, IViewFor<ConnectionViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ConnectionViewModel), typeof(ConnectionView), new PropertyMetadata(null));

        public ConnectionViewModel ViewModel
        {
            get => (ConnectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ConnectionViewModel)value;
        }
        #endregion

        #region States
        #region HighlightStates
        public const string HighlightVisualStatesGroup = "HighlightStates";
        public const string HighlightedState = "Highlighted";
        public const string NonHighlightedState = "NonHighlighted";
        #endregion

        #region ErrorStates
        public const string ErrorVisualStatesGroup = "ErrorStates";
        public const string ErrorState = "Error";
        public const string NonErrorState = "NoError";
        #endregion

        #region ErrorStates
        public const string MarkedForDeleteVisualStatesGroup = "MarkedForDeleteStates";
        public const string MarkedForDeleteState = "Marked";
        public const string NotMarkedForDeleteState = "NotMarked";
        #endregion
        #endregion

        #region RegularBrush
        public Brush RegularBrush
        {
            get => (Brush)this.GetValue(RegularBrushProperty);
            set => this.SetValue(RegularBrushProperty, value);
        }
        public static readonly DependencyProperty RegularBrushProperty = DependencyProperty.Register(nameof(RegularBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region ErrorBrush
        public Brush ErrorBrush
        {
            get => (Brush)this.GetValue(ErrorBrushProperty);
            set => this.SetValue(ErrorBrushProperty, value);
        }
        public static readonly DependencyProperty ErrorBrushProperty = DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region HighlightBrush
        public Brush HighlightBrush
        {
            get => (Brush)this.GetValue(HighlightBrushProperty);
            set => this.SetValue(HighlightBrushProperty, value);
        }
        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region MarkedForDeleteBrush
        public Brush MarkedForDeleteBrush
        {
            get => (Brush)this.GetValue(MarkedForDeleteBrushProperty);
            set => this.SetValue(MarkedForDeleteBrushProperty, value);
        }
        public static readonly DependencyProperty MarkedForDeleteBrushProperty =
            DependencyProperty.Register(nameof(MarkedForDeleteBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region Geometry
        public Geometry Geometry
        {
            get => (Geometry)this.GetValue(GeometryProperty);
            private set => this.SetValue(GeometryProperty, value);
        }
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(nameof(Geometry), typeof(Geometry), typeof(ConnectionView));
        #endregion
        
        public ConnectionView()
        {
            this.DefaultStyleKey = typeof(ConnectionView);
            
            SetupPathData();
            SetupBrushesBinding();
        }

        public override void OnApplyTemplate()
        {
            VisualStateManager.GoToState(this, NonHighlightedState, false);
            VisualStateManager.GoToState(this, NonErrorState, false);
            VisualStateManager.GoToState(this, NotMarkedForDeleteState, false);
        }

        private void SetupPathData()
        {
            this.WhenActivated(d => d(
                this.WhenAny(v => v.ViewModel.Input.Port.CenterPoint, v => v.ViewModel.Output.Port.CenterPoint, (a, b) => (a, b))
                    .Select(_ => BuildSmoothBezier(ViewModel.Input.Port.CenterPoint, ViewModel.Output.Port.CenterPoint))
                    .BindTo(this, v => v.Geometry)
            ));
        }

        private void SetupBrushesBinding()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel.IsHighlighted).Subscribe(isHighlighted =>
                {
                    VisualStateManager.GoToState(this, isHighlighted ? HighlightedState : NonHighlightedState, true);
                }).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.IsInErrorState).Subscribe(isInErrorState =>
                {
                    VisualStateManager.GoToState(this, isInErrorState ? ErrorState : NonErrorState, true);
                }).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.IsMarkedForDelete).Subscribe(isMarkedForDelete =>
                {
                    VisualStateManager.GoToState(this, isMarkedForDelete ? MarkedForDeleteState : NotMarkedForDeleteState, true);
                }).DisposeWith(d);
            });
        }

        public static PathGeometry BuildSmoothBezier(Point startPoint, Point endPoint)
        {
            double width = endPoint.X - startPoint.X;
            double height = endPoint.Y - startPoint.Y;
            Point p1 = startPoint;
            Point p2 = new Point(p1.X + (width / 4d), p1.Y);
            Point p3 = new Point(p1.X + (width / 2d), p1.Y + (height / 2d));
            Point p4 = new Point(p1.X + (3d * width / 4d), endPoint.Y);
            Point p5 = endPoint;

            PathFigure pathFigure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                Segments =
                {
                    new BezierSegment(p1, p2, p3, true),
                    new BezierSegment(p3, p4, p5, true)
                }
            };

            PathGeometry geom = new PathGeometry();
            geom.Figures.Add(pathFigure);

            return geom;
        }
    }
}
