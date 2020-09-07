using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;

using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(InputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(OutputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(EndpointGroupsList), Type = typeof(ItemsControl))]
    public class EndpointGroupView : ReactiveUserControl<EndpointGroupViewModel>
    {
        private TextBlock NameLabel { get; set; }
        private ItemsControl InputsList { get; set; }
        private ItemsControl OutputsList { get; set; }
        private ItemsControl EndpointGroupsList { get; set; }

        #region Properties
        public static readonly DependencyProperty TitleFontFamilyProperty = DependencyProperty.Register(nameof(TitleFontFamily), typeof(FontFamily), typeof(EndpointGroupView));
        public FontFamily TitleFontFamily
        {
            get => (FontFamily)GetValue(TitleFontFamilyProperty);
            set => SetValue(TitleFontFamilyProperty, value);
        }

        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(EndpointGroupView));
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }
        #endregion

        public EndpointGroupView()
        {
            DefaultStyleKey = typeof(EndpointGroupView);

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Group.Name, v => v.NameLabel.Text).DisposeWith(d);

	            this.BindList(ViewModel, vm => vm.VisibleInputs, v => v.InputsList.ItemsSource).DisposeWith(d);
	            this.BindList(ViewModel, vm => vm.VisibleOutputs, v => v.OutputsList.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Children, v => v.EndpointGroupsList.ItemsSource).DisposeWith(d);
            });
        }

        public override void OnApplyTemplate()
        {
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBlock;
            InputsList = GetTemplateChild(nameof(InputsList)) as ItemsControl;
            OutputsList = GetTemplateChild(nameof(OutputsList)) as ItemsControl;
            EndpointGroupsList = GetTemplateChild(nameof(EndpointGroupsList)) as ItemsControl;
        }
    }
}
