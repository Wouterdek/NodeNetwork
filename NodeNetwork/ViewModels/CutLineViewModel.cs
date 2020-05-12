using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for the UI cutting line that is used to delete connections.
    /// </summary>
    [DataContract]
    public class CutLineViewModel : ReactiveObject
    {
        #region StartPoint
        /// <summary>
        /// The coordinates of the point at which the cutting line starts.
        /// </summary>
        [DataMember]
        public Point StartPoint
        {
            get => _startPoint;
            set => this.RaiseAndSetIfChanged(ref _startPoint, value);
        }
        [IgnoreDataMember] private Point _startPoint;
        #endregion

        #region EndPoint
        /// <summary>
        /// The coordinates of the point at which the cutting line ends.
        /// </summary>
        [DataMember]
        public Point EndPoint
        {
            get => _endPoint;
            set => this.RaiseAndSetIfChanged(ref _endPoint, value);
        }
        [IgnoreDataMember] private Point _endPoint;
        #endregion

        #region IsVisible
        /// <summary>
        /// If true, the cutting line is visible. If false, the cutting line is hidden.
        /// </summary>
        [DataMember]
        public bool IsVisible
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }
        [IgnoreDataMember] private bool _isVisible;
        #endregion

        #region IntersectingConnections
        /// <summary>
        /// A list of connections that visually intersect with the cutting line.
        /// This list is driven by the view.
        /// </summary>
        [DataMember] public ISourceList<ConnectionViewModel> IntersectingConnections { get; set; } = new SourceList<ConnectionViewModel>();
        #endregion
    }
}
