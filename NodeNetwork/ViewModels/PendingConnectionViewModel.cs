using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class PendingConnectionViewModel : ReactiveObject
    {
        static PendingConnectionViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new PendingConnectionView(), typeof(IViewFor<PendingConnectionViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public NetworkViewModel Parent { get; }

        #region Input
        private NodeInputViewModel _input;
        public NodeInputViewModel Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }
        #endregion

        #region InputIsLocked
        private bool _inputIsLocked;
        public bool InputIsLocked
        {
            get => _inputIsLocked;
            set => this.RaiseAndSetIfChanged(ref _inputIsLocked, value);
        }
        #endregion

        #region Output
        private NodeOutputViewModel _output;
        public NodeOutputViewModel Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        #endregion
        
        #region OutputIsLocked
        private bool _outputIsLocked;
        public bool OutputIsLocked
        {
            get => _outputIsLocked;
            set => this.RaiseAndSetIfChanged(ref _outputIsLocked, value);
        }
        #endregion

        #region LooseEndPoint
        private Point _looseEndPoint;
        public Point LooseEndPoint
        {
            get => _looseEndPoint;
            set => this.RaiseAndSetIfChanged(ref _looseEndPoint, value);
        }
        #endregion

        #region BoundingBox
        private readonly ObservableAsPropertyHelper<Rect> _boundingBox;
        public Rect BoundingBox => _boundingBox.Value;
        #endregion

        #region Validation
        private ConnectionValidationResult _validation;
        public ConnectionValidationResult Validation
        {
            get => _validation;
            set => this.RaiseAndSetIfChanged(ref _validation, value);
        }
        #endregion
        
        public PendingConnectionViewModel(NetworkViewModel parent)
        {
            Parent = parent;
            this.WhenAnyValue(vm => vm.Input, vm => vm.Output, vm => vm.LooseEndPoint)
                .Select(_ =>
                {
                    Point p1 = Output?.Port.CenterPoint ?? LooseEndPoint;
                    Point p2 = Input?.Port.CenterPoint ?? LooseEndPoint;
                    return new Rect(p1, p2);
                }).ToProperty(this, vm => vm.BoundingBox, out _boundingBox);
        }
    }
}
