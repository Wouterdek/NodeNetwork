using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class CutLineViewModel : ReactiveObject
    {
        #region StartPoint
        private Point _startPoint;
        public Point StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }
        #endregion

        #region PortView
        private Point _endPoint;
        public Point EndPoint
        {
            get => _endPoint;
            set => this.RaiseAndSetIfChanged(ref _endPoint, value);
        }
        #endregion

        #region IsVisible
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
        #endregion

        #region IntersectingConnections
        public ReactiveList<ConnectionViewModel> IntersectingConnections { get; } = new ReactiveList<ConnectionViewModel>();
        #endregion
    }
}
