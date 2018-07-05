using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(TextBlock), Type = typeof(TextBlock))]
    public class ErrorMessageView : Control, IViewFor<ErrorMessageViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ErrorMessageViewModel), typeof(ErrorMessageView), new PropertyMetadata(null));

        public ErrorMessageViewModel ViewModel
        {
            get => (ErrorMessageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ErrorMessageViewModel)value;
        }
        #endregion

        private TextBlock TextBlock { get; set; }

        public ErrorMessageView()
        {
            DefaultStyleKey = typeof(ErrorMessageView);

            SetupBindings();
        }

        private void SetupBindings()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Message, v => v.TextBlock.Text).DisposeWith(d);
            });
        }

        public override void OnApplyTemplate()
        {
            TextBlock = GetTemplateChild(nameof(TextBlock)) as TextBlock;
        }
    }
}
