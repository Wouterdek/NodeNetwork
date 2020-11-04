using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NodeNetwork.ViewModels;
using NodeNetwork.Views.Controls;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplateVisualState(Name = ConnectedState, GroupName = ConnectedVisualStatesGroup)]
    [TemplateVisualState(Name = DisconnectedState, GroupName = ConnectedVisualStatesGroup)]
    [TemplateVisualState(Name = HighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = NonHighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = ErrorState, GroupName = ErrorVisualStatesGroup)]
    [TemplateVisualState(Name = NonErrorState, GroupName = ErrorVisualStatesGroup)]
    public class PortView : Control, IViewFor<PortViewModel>
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

        #region ConnectedStates
        public const string ConnectedVisualStatesGroup = "ConnectedStates";
        public const string ConnectedState = "Connected";
        public const string DisconnectedState = "Disconnected";
        #endregion

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

        #region Brushes
        #region RegularStroke
        public Brush RegularStroke
        {
            get => (Brush)this.GetValue(RegularStrokeProperty);
            set => this.SetValue(RegularStrokeProperty, value);
        }
        public static readonly DependencyProperty RegularStrokeProperty = DependencyProperty.Register(nameof(RegularStroke), typeof(Brush), typeof(PortView));
        #endregion

        #region RegularFill
        public Brush RegularFill
        {
            get => (Brush)this.GetValue(RegularFillProperty);
            set => this.SetValue(RegularFillProperty, value);
        }
        public static readonly DependencyProperty RegularFillProperty = DependencyProperty.Register(nameof(RegularFill), typeof(Brush), typeof(PortView));
        #endregion

        #region ConnectedStroke
        public Brush ConnectedStroke
        {
            get => (Brush)this.GetValue(ConnectedStrokeProperty);
            set => this.SetValue(ConnectedStrokeProperty, value);
        }
        public static readonly DependencyProperty ConnectedStrokeProperty = DependencyProperty.Register(nameof(ConnectedStroke), typeof(Brush), typeof(PortView));
        #endregion

        #region ConnectedFill
        public Brush ConnectedFill
        {
            get => (Brush)this.GetValue(ConnectedFillProperty);
            set => this.SetValue(ConnectedFillProperty, value);
        }
        public static readonly DependencyProperty ConnectedFillProperty = DependencyProperty.Register(nameof(ConnectedFill), typeof(Brush), typeof(PortView));
        #endregion

        #region HighlightStroke
        public Brush HighlightStroke
        {
            get => (Brush)this.GetValue(HighlightStrokeProperty);
            set => this.SetValue(HighlightStrokeProperty, value);
        }
        public static readonly DependencyProperty HighlightStrokeProperty = DependencyProperty.Register(nameof(HighlightStroke), typeof(Brush), typeof(PortView));
        #endregion

        #region HighlightFill
        public Brush HighlightFill
        {
            get => (Brush)this.GetValue(HighlightFillProperty);
            set => this.SetValue(HighlightFillProperty, value);
        }
        public static readonly DependencyProperty HighlightFillProperty = DependencyProperty.Register(nameof(HighlightFill), typeof(Brush), typeof(PortView));
        #endregion

        #region ErrorStroke
        public Brush ErrorStroke
        {
            get => (Brush)this.GetValue(ErrorStrokeProperty);
            set => this.SetValue(ErrorStrokeProperty, value);
        }
        public static readonly DependencyProperty ErrorStrokeProperty = DependencyProperty.Register(nameof(ErrorStroke), typeof(Brush), typeof(PortView));
        #endregion

        #region ErrorFill
        public Brush ErrorFill
        {
            get => (Brush)this.GetValue(ErrorFillProperty);
            set => this.SetValue(ErrorFillProperty, value);
        }
        public static readonly DependencyProperty ErrorFillProperty = DependencyProperty.Register(nameof(ErrorFill), typeof(Brush), typeof(PortView));
        #endregion
        #endregion
        
        public PortView()
        {
            this.DefaultStyleKey = typeof(PortView);
        }

        public override void OnApplyTemplate()
        {
            VisualStateManager.GoToState(this, DisconnectedState, false);
            VisualStateManager.GoToState(this, NonHighlightedState, false);
            VisualStateManager.GoToState(this, NonErrorState, false);

            SetupLayoutEvent();
            SetupMouseEvents();
            SetupVisualStateBindings();
        }

        private void SetupVisualStateBindings()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyObservable(v => v.ViewModel.Parent.Connections.CountChanged).Select(c => c == 0).Subscribe(isDisconnected =>
                {
                    VisualStateManager.GoToState(this, isDisconnected ? DisconnectedState : ConnectedState, true);
                }).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.IsHighlighted).Subscribe(isHighlighted =>
                {
                    VisualStateManager.GoToState(this, isHighlighted ? HighlightedState : NonHighlightedState, true);
                }).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.IsInErrorMode).Subscribe(isInErrorMode =>
                {
                    VisualStateManager.GoToState(this, isInErrorMode ? ErrorState : NonErrorState, true);
                }).DisposeWith(d);
            });
        }

        protected virtual void SetupLayoutEvent()
        {
	        this.WhenActivated(d =>
	        {
		        this.Events().LayoutUpdated.Subscribe(e =>
		        {
			        //Update endpoint center point
			        if (ViewModel == null)
			        {
				        return;
			        }

			        NetworkView networkView = WPFUtils.FindParent<NetworkView>(this);
			        if (networkView == null)
			        {
				        return;
			        }

			        Point center = new Point(this.ActualWidth / 2d, this.ActualHeight / 2d);
			        if (Margin.Left < 0)
			        {
				        center.X += Margin.Left;
			        }
			        else if (Margin.Right < 0)
			        {
				        center.X -= Margin.Right;
			        }

			        var transform = this.TransformToAncestor(networkView.contentContainer);
			        ViewModel.CenterPoint = transform.Transform(center);
				}).DisposeWith(d);
	        });
        }

        private void SetupMouseEvents()
        {
            this.MouseLeftButtonDown += (sender, e) =>
            {
                e.Handled = true;
                ViewModel.OnDragFromPort();
            };
            this.MouseEnter += (sender, e) =>
            {
                e.Handled = true;
                ViewModel.OnPortEnter();
            };
            this.MouseLeave += (sender, e) =>
            {
                e.Handled = true;
                ViewModel.OnPortLeave();
            };
            this.MouseLeftButtonUp += (sender, e) =>
            {
                e.Handled = true;
                ViewModel.OnDropOnPort();
            };
        }
    }
}
