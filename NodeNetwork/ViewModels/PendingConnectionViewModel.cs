﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel for a connection that is currently being build by the user.
    /// </summary>
    [DataContract]
    public class PendingConnectionViewModel : ReactiveObject
    {
        static PendingConnectionViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new PendingConnectionView(), typeof(IViewFor<PendingConnectionViewModel>));
        }

        #region Logger
        [IgnoreDataMember] private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The network viewmodel that this connection is being build in.
        /// </summary>
        [DataMember] public NetworkViewModel Parent { get; set; }
        #endregion

        #region Input
        /// <summary>
        /// The node input viewmodel, if any, that is on one side of the connection.
        /// Can be null.
        /// </summary>
        [DataMember]
        public NodeInputViewModel Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }
        [IgnoreDataMember] private NodeInputViewModel _input;
        #endregion

        #region InputIsLocked
        /// <summary>
        /// If true, Input will not be changed. 
        /// This is used to mark Input as the starting point of the pending connection.
        /// </summary>
        [DataMember]
        public bool InputIsLocked
        {
            get => _inputIsLocked;
            set => this.RaiseAndSetIfChanged(ref _inputIsLocked, value);
        }
        [IgnoreDataMember] private bool _inputIsLocked;
        #endregion

        #region Output
        /// <summary>
        /// The node output viewmodel, if any, that is on one side of the connection.
        /// Can be null.
        /// </summary>
        [DataMember]
        public NodeOutputViewModel Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        [IgnoreDataMember] private NodeOutputViewModel _output;
        #endregion

        #region OutputIsLocked
        /// <summary>
        /// If true, Output will not be changed. 
        /// This is used to mark Output as the starting point of the pending connection.
        /// </summary>
        [DataMember]
        public bool OutputIsLocked
        {
            get => _outputIsLocked;
            set => this.RaiseAndSetIfChanged(ref _outputIsLocked, value);
        }
        [IgnoreDataMember] private bool _outputIsLocked;
        #endregion

        #region LooseEndPoint
        /// <summary>
        /// The current coordinates of the point where the pending connection ends on the loose side.
        /// This value is used when the Input or Output is null.
        /// </summary>
        [DataMember]
        public Point LooseEndPoint
        {
            get => _looseEndPoint;
            set => this.RaiseAndSetIfChanged(ref _looseEndPoint, value);
        }
        [IgnoreDataMember] private Point _looseEndPoint;
        #endregion

        #region BoundingBox
        /// <summary>
        /// The rectangle that contains the entire connection view.
        /// </summary>
        [IgnoreDataMember] public Rect BoundingBox => _boundingBox.Value;
        [IgnoreDataMember] private readonly ObservableAsPropertyHelper<Rect> _boundingBox;
        #endregion

        #region Validation
        /// <summary>
        /// The validation of the current connection state. 
        /// If invalid, the connection will be displayed as such and an error message will be displayed.
        /// The pending connection must be valid before it can be added to the network as a real connection.
        /// </summary>
        [DataMember]
        public ConnectionValidationResult Validation
        {
            get => _validation;
            set => this.RaiseAndSetIfChanged(ref _validation, value);
        }
        [IgnoreDataMember] private ConnectionValidationResult _validation;
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
