using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Reactive.Linq;

namespace NodeNetwork.Views
{
    public partial class PendingConnectionView : IViewFor<PendingConnectionViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(PendingConnectionViewModel), typeof(PendingConnectionView), new PropertyMetadata(null));

        public PendingConnectionViewModel ViewModel
        {
            get => (PendingConnectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PendingConnectionViewModel)value;
        }
        #endregion
        
        #region LineBrush
        public Brush LineBrush
        {
            get => (Brush)this.GetValue(LineBrushProperty);
            set => this.SetValue(LineBrushProperty, value);
        }
        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(PendingConnectionView), new PropertyMetadata());
        #endregion
        
        #region ErrorBrush
        public Brush ErrorBrush
        {
            get => (Brush)this.GetValue(ErrorBrushProperty);
            set => this.SetValue(ErrorBrushProperty, value);
        }
        public static readonly DependencyProperty ErrorBrushProperty = DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(PendingConnectionView), new PropertyMetadata());
        #endregion
        
        public PendingConnectionView()
        {
            InitializeComponent();
            
            this.LineBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            this.ErrorBrush = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));

            SetupPathData();
            SetupColor();
        }

        private void SetupPathData()
        {
            this.WhenAnyValue(v => v.ViewModel.LooseEndPoint)
                .Select(_ =>
                {
                    if (ViewModel.Input == null)
                    {
                        return ConnectionView.BuildSmoothBezier(ViewModel.Output.Port.CenterPoint,
                            ViewModel.LooseEndPoint);
                    }
                    else if (ViewModel.Output == null)
                    {
                        return ConnectionView.BuildSmoothBezier(ViewModel.LooseEndPoint,
                            ViewModel.Input.Port.CenterPoint);
                    }
                    else
                    {
                        return ConnectionView.BuildSmoothBezier(ViewModel.Output.Port.CenterPoint,
                            ViewModel.Input.Port.CenterPoint);
                    }
                })
                .BindTo(this, v => v.path.Data);
        }

        private void SetupColor()
        {
            this.WhenActivated(d => d(
                this.WhenAnyValue(v => v.ViewModel.Validation)
                    .Select(_ =>
                    {
                        if (ViewModel.Validation == null || ViewModel.Validation.IsValid)
                        {
                            return LineBrush;
                        }
                        else
                        {
                            return ErrorBrush;
                        }
                    }).BindTo(this, v => v.path.Stroke)
            ));
        }
    }
}
