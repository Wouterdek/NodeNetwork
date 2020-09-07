using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using NodeNetwork.Views.Controls;
using ReactiveUI;
using Splat;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(CollapseButton), Type = typeof(ArrowToggleButton))]
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(HeaderIcon), Type = typeof(Image))]
    [TemplatePart(Name = nameof(InputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(OutputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(EndpointGroupsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(ResizeVerticalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeHorizontalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalThumb), Type = typeof(Thumb))]
    [TemplateVisualState(Name = SelectedState, GroupName = SelectedVisualStatesGroup)]
    [TemplateVisualState(Name = UnselectedState, GroupName = SelectedVisualStatesGroup)]
    [TemplateVisualState(Name = CollapsedState, GroupName = CollapsedVisualStatesGroup)]
    [TemplateVisualState(Name = ExpandedState, GroupName = CollapsedVisualStatesGroup)]
    public class NodeView : Control, IViewFor<NodeViewModel>
    {
        #region SelectedStates
        public const string SelectedVisualStatesGroup = "SelectedStates";
        public const string SelectedState = "Selected";
        public const string UnselectedState = "Unselected";
        #endregion

        #region CollapsedStates
        public const string CollapsedVisualStatesGroup = "CollapsedStates";
        public const string CollapsedState = "Collapsed";
        public const string ExpandedState = "Expanded"; 
        #endregion

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeViewModel), typeof(NodeView), new PropertyMetadata(null));

        public NodeViewModel ViewModel
        {
            get => (NodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeViewModel)value;
        }
        #endregion

        #region Properties
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(NodeView));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(nameof(ArrowSize), typeof(double), typeof(NodeView));
        public double ArrowSize
        {
            get => (double)GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        public static readonly DependencyProperty TitleFontFamilyProperty = DependencyProperty.Register(nameof(TitleFontFamily), typeof(FontFamily), typeof(NodeView));
        public FontFamily TitleFontFamily
        {
            get => (FontFamily)GetValue(TitleFontFamilyProperty);
            set => SetValue(TitleFontFamilyProperty, value);
        }

        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(NodeView));
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty EndpointsStackingOrientationProperty = DependencyProperty.Register(nameof(EndpointsStackingOrientation), typeof(Orientation), typeof(NodeView));
        public Orientation EndpointsStackingOrientation
        {
            get => (Orientation)GetValue(EndpointsStackingOrientationProperty);
            set => SetValue(EndpointsStackingOrientationProperty, value);
        }

        public static readonly DependencyProperty LeadingControlPresenterStyleProperty = DependencyProperty.Register(nameof(LeadingControlPresenterStyle), typeof(Style), typeof(NodeView));
        public Style LeadingControlPresenterStyle
        {
	        get => (Style)GetValue(LeadingControlPresenterStyleProperty);
	        set => SetValue(LeadingControlPresenterStyleProperty, value);
        }

        public static readonly DependencyProperty TrailingControlPresenterStyleProperty = DependencyProperty.Register(nameof(TrailingControlPresenterStyle), typeof(Style), typeof(NodeView));
        public Style TrailingControlPresenterStyle
		{
	        get => (Style)GetValue(TrailingControlPresenterStyleProperty);
	        set => SetValue(TrailingControlPresenterStyleProperty, value);
        }
		#endregion

		private ArrowToggleButton CollapseButton { get; set; }
        private TextBlock NameLabel { get; set; }
        private Image HeaderIcon { get; set; }
        private ItemsControl InputsList { get; set; }
        private ItemsControl OutputsList { get; set; }
        private ItemsControl EndpointGroupsList { get; set; }
        private Thumb ResizeVerticalThumb { get; set; }
        private Thumb ResizeHorizontalThumb { get; set; }
        private Thumb ResizeDiagonalThumb { get; set; }

        public NodeView()
        {
            DefaultStyleKey = typeof(NodeView);

            SetupBindings();
            SetupEvents();
            SetupVisualStateBindings();
        }

        public override void OnApplyTemplate()
        {
            CollapseButton = GetTemplateChild(nameof(CollapseButton)) as ArrowToggleButton;
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBlock;
            HeaderIcon = GetTemplateChild(nameof(HeaderIcon)) as Image;
            InputsList = GetTemplateChild(nameof(InputsList)) as ItemsControl;
            OutputsList = GetTemplateChild(nameof(OutputsList)) as ItemsControl;
            EndpointGroupsList = GetTemplateChild(nameof(EndpointGroupsList)) as ItemsControl;
            ResizeVerticalThumb = GetTemplateChild(nameof(ResizeVerticalThumb)) as Thumb;
            ResizeHorizontalThumb = GetTemplateChild(nameof(ResizeHorizontalThumb)) as Thumb;
            ResizeDiagonalThumb = GetTemplateChild(nameof(ResizeDiagonalThumb)) as Thumb;

            ResizeVerticalThumb.DragDelta += (sender, e) => ApplyResize(e, false, true);
            ResizeHorizontalThumb.DragDelta += (sender, e) => ApplyResize(e, true, false);
            ResizeDiagonalThumb.DragDelta += (sender, e) => ApplyResize(e, true, true);

            VisualStateManager.GoToState(this, ExpandedState, false);
            VisualStateManager.GoToState(this, UnselectedState, false);
        }

        private void ApplyResize(DragDeltaEventArgs e, bool horizontal, bool vertical)
        {
            if (horizontal)
            {
                MinWidth = Math.Max(20, MinWidth + e.HorizontalChange);
            }
            if (vertical)
            {
                MinHeight = Math.Max(20, MinHeight + e.VerticalChange);
            }
        }

        private void SetupBindings()
        {
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.IsCollapsed, v => v.CollapseButton.IsChecked).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameLabel.Text).DisposeWith(d);

	            this.BindList(ViewModel, vm => vm.VisibleInputs, v => v.InputsList.ItemsSource).DisposeWith(d);
	            this.BindList(ViewModel, vm => vm.VisibleOutputs, v => v.OutputsList.ItemsSource).DisposeWith(d);
	            this.OneWayBind(ViewModel, vm => vm.VisibleEndpointGroups, v => v.EndpointGroupsList.ItemsSource).DisposeWith(d);

                this.WhenAnyValue(v => v.ActualWidth, v => v.ActualHeight, (width, height) => new Size(width, height))
                    .BindTo(this, v => v.ViewModel.Size).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.HeaderIcon, v => v.HeaderIcon.Source, img => img?.ToNative()).DisposeWith(d);
            });
        }

        private void SetupEvents()
        {
            this.MouseLeftButtonDown += (sender, args) =>
            {
                this.Focus();

                if (ViewModel == null)
                {
                    return;
                }

                if (ViewModel.IsSelected)
                {
                    return;
                }

                if (ViewModel.Parent != null && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ViewModel.Parent.ClearSelection();
                }

                ViewModel.IsSelected = true;
            };
        }

        private void SetupVisualStateBindings()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel.IsCollapsed).Subscribe(isCollapsed =>
                {
                    VisualStateManager.GoToState(this, isCollapsed ? CollapsedState : ExpandedState, true);
                }).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.IsSelected).Subscribe(isSelected =>
                {
                    VisualStateManager.GoToState(this, isSelected ? SelectedState : UnselectedState, true);
                }).DisposeWith(d);
            });
        }
    }
}
