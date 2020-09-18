using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;

namespace NodeNetwork.Toolkit.Group.AddEndpointDropPanel
{
    public partial class AddEndpointDropPanelView : IViewFor<AddEndpointDropPanelViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AddEndpointDropPanelViewModel), typeof(AddEndpointDropPanelView), new PropertyMetadata(null));

        public AddEndpointDropPanelViewModel ViewModel
        {
            get => (AddEndpointDropPanelViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AddEndpointDropPanelViewModel)value;
        }
        #endregion

        #region DropHintText
        public static readonly DependencyProperty DropHintTextProperty = DependencyProperty.Register(nameof(DropHintText),
            typeof(string), typeof(AddEndpointDropPanelView), new PropertyMetadata(null));

        public string DropHintText
        {
            get => (string)GetValue(DropHintTextProperty);
            set => SetValue(DropHintTextProperty, value);
        }
        #endregion

        public AddEndpointDropPanelView()
        {
            InitializeComponent();
            DropHintText = "Drop here to create new entry";

            this.WhenActivated(d =>
            {
                this.Events().MouseLeftButtonUp
                    .Select(_ => Unit.Default)
                    .InvokeCommand(this, v => v.ViewModel.AddEndpointFromPendingConnection)
                    .DisposeWith(d);
            });
        }
    }
}
