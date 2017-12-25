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
using NodeNetwork.Views.Controls;
using ReactiveUI;

namespace NodeNetwork.Views
{
    public partial class PortView : UserControl, IViewFor<PortViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(PortViewModel), typeof(PortView), new PropertyMetadata(null));

        public PortViewModel ViewModel
        {
            get => (PortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PortViewModel)value;
        }
        #endregion
        
        #region RegularStroke
        public Brush RegularStroke
        {
            get => (Brush)this.GetValue(RegularStrokeProperty);
            set => this.SetValue(RegularStrokeProperty, value);
        }
        public static readonly DependencyProperty RegularStrokeProperty = DependencyProperty.Register(nameof(RegularStroke), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion

        #region RegularFill
        public Brush RegularFill
        {
            get => (Brush)this.GetValue(RegularFillProperty);
            set => this.SetValue(RegularFillProperty, value);
        }
        public static readonly DependencyProperty RegularFillProperty = DependencyProperty.Register(nameof(RegularFill), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion
        
        #region HighlightStroke
        public Brush HighlightStroke
        {
            get => (Brush)this.GetValue(HighlightStrokeProperty);
            set => this.SetValue(HighlightStrokeProperty, value);
        }
        public static readonly DependencyProperty HighlightStrokeProperty = DependencyProperty.Register(nameof(HighlightStroke), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion

        #region HighlightFill
        public Brush HighlightFill
        {
            get => (Brush)this.GetValue(HighlightFillProperty);
            set => this.SetValue(HighlightFillProperty, value);
        }
        public static readonly DependencyProperty HighlightFillProperty = DependencyProperty.Register(nameof(HighlightFill), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion
        
        #region ErrorStroke
        public Brush ErrorStroke
        {
            get => (Brush)this.GetValue(ErrorStrokeProperty);
            set => this.SetValue(ErrorStrokeProperty, value);
        }
        public static readonly DependencyProperty ErrorStrokeProperty = DependencyProperty.Register(nameof(ErrorStroke), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion

        #region ErrorFill
        public Brush ErrorFill
        {
            get => (Brush)this.GetValue(ErrorFillProperty);
            set => this.SetValue(ErrorFillProperty, value);
        }
        public static readonly DependencyProperty ErrorFillProperty = DependencyProperty.Register(nameof(ErrorFill), typeof(Brush), typeof(PortView), new PropertyMetadata());
        #endregion

        public PortView()
        {
            InitializeComponent();

            RegularStroke = new SolidColorBrush(Color.FromRgb(158, 158, 158));
            RegularFill = new SolidColorBrush(Color.FromRgb(224, 224, 224));

            HighlightStroke = new SolidColorBrush(Color.FromRgb(178, 178, 178));
            HighlightFill = new SolidColorBrush(Color.FromRgb(242, 242, 242));

            ErrorStroke = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            ErrorFill = new SolidColorBrush(Color.FromRgb(255, 205, 210));

            this.WhenAnyValue(v => v.ViewModel.IsHighlighted, v => v.ViewModel.IsInErrorMode)
                .Select(_ =>
                {
                    if (ViewModel.IsInErrorMode)
                    {
                        return ErrorStroke;
                    }
                    if (ViewModel.IsHighlighted)
                    {
                        return HighlightStroke;
                    }
                    return RegularStroke;
                })
                .BindTo(this, v => v.PointEllipse.Stroke);

            this.WhenAnyValue(v => v.ViewModel.IsHighlighted, v => v.ViewModel.IsInErrorMode)
                .Select(_ => 
                {
                    if (ViewModel.IsInErrorMode)
                    {
                        return ErrorFill;
                    }
                    if (ViewModel.IsHighlighted)
                    {
                        return HighlightFill;
                    }
                    return RegularFill;
                })
                .BindTo(this, v => v.PointEllipse.Fill);

            this.LayoutUpdated += OnLayoutUpdated;
            this.MouseLeftButtonDown += OnMouseLeftButtonDown;
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += OnMouseLeave;
            this.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            //Update endpoint center point
            NetworkView	networkView = WPFUtils.FindParent<NetworkView>(this);
            if (networkView == null)
            {
                return;
            }

            Point center = new Point(this.ActualWidth / 2d, this.ActualHeight / 2d);
            if (Margin.Left < 0)
            {
                center.X += Margin.Left;
            }else if (Margin.Right < 0)
            {
                center.X -= Margin.Right;
            }
            ViewModel.CenterPoint = this.TranslatePoint(center, networkView.contentContainer);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnDragFromPort();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnPortEnter();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnPortLeave();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ViewModel.OnDropOnPort();
        }
    }
}
