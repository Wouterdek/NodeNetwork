using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    public partial class ConnectionView : IViewFor<ConnectionViewModel>
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

        public ConnectionView()
        {
            InitializeComponent();

            SetupDefaultBrushes();
            SetupPathData();
            SetupColor();
        }

        private void SetupDefaultBrushes()
        {
            RegularBrush = new SolidColorBrush(Colors.White);
            ErrorBrush = new SolidColorBrush(Colors.DarkRed);
            HighlightBrush = new SolidColorBrush(Colors.Yellow);
            MarkedForDeleteBrush = new SolidColorBrush(Colors.Red);
        }

        private void SetupPathData()
        {
            this.WhenAny(v => v.ViewModel.Input.Port.CenterPoint, v => v.ViewModel.Output.Port.CenterPoint, (a, b) => (a, b))
                .Select(_ => BuildSmoothBezier(ViewModel.Input.Port.CenterPoint, ViewModel.Output.Port.CenterPoint))
                .BindTo(this, v => v.path.Data);
        }

        private void SetupColor()
        {
            this.WhenActivated(d => d(
                this.WhenAnyValue(v => v.ViewModel.IsHighlighted, v => v.ViewModel.IsInErrorState, v => v.ViewModel.IsMarkedForDelete)
                .Select(_ =>
                {
                    if (ViewModel.IsMarkedForDelete)
                    {
                        return MarkedForDeleteBrush;
                    }
                    else if (ViewModel.IsHighlighted)
                    {
                        return HighlightBrush;
                    }
                    else if (ViewModel.IsInErrorState)
                    {
                        return ErrorBrush;
                    }
                    else
                    {
                        return RegularBrush;
                    }
                }).BindTo(this, v => v.path.Stroke)
            ));
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
