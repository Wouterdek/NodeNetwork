using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for inputs on a node.
    /// Inputs are endpoints that can only be connected to outputs.
    /// </summary>
    public class NodeInputViewModel : Endpoint
    {
        static NodeInputViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeInputView(), typeof(IViewFor<NodeInputViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region IsEditorVisible
        /// <summary>
        /// If true, the editor is visible. Otherwise, the editor is hidden.
        /// See HideEditorIfConnected.
        /// </summary>
        public bool IsEditorVisible => _isEditorVisible.Value;
        private ObservableAsPropertyHelper<bool> _isEditorVisible;
        #endregion

        #region HideEditorIfConnected
        /// <summary>
        /// If true, the editor of this input will be hidden if Connection is not null.
        /// This makes sense if the editor is used to provide a value when no connection is present.
        /// </summary>
        public bool HideEditorIfConnected
        {
            get => _hideEditorIfConnected;
            set => this.RaiseAndSetIfChanged(ref _hideEditorIfConnected, value);
        }
        private bool _hideEditorIfConnected;
        #endregion
        
        #region ConnectionValidator
        /// <summary>
        /// This function is called when a new connection with this input is pending.
        /// It decides whether or not the pending connection is valid.
        /// If the validation result says the pending connection is invalid, 
        /// then the user will not be able to add the connection to the network.
        /// </summary>
        public Func<PendingConnectionViewModel, ConnectionValidationResult> ConnectionValidator
        {
            get => _connectionValidator;
            set => this.RaiseAndSetIfChanged(ref _connectionValidator, value);
        }
        private Func<PendingConnectionViewModel, ConnectionValidationResult> _connectionValidator;
        #endregion
        
        public NodeInputViewModel()
        {
            this.HideEditorIfConnected = true;

            this.Connections.CountChanged.Select(c => c == 0).StartWith(true)
                .CombineLatest(this.WhenAnyValue(vm => vm.HideEditorIfConnected), (noConnections, hideEditorIfConnected) => !hideEditorIfConnected || noConnections)
                .ToProperty(this, vm => vm.IsEditorVisible, out _isEditorVisible);

            this.ConnectionValidator = con => new ConnectionValidationResult(true, null);

            this.MaxConnections = 1;
            this.PortPosition = PortPosition.Left;
        }

        /// <summary>
        /// Sets the pending connection in the network to a new connection with this endpoint as the input.
        /// If this input already is connected, and MaxConnections == 1,
        /// then the connection is replaced by a pending connection without this endpoint.
        /// If the connection would be invalid, no pending connection is made.
        /// Called when the user clicks on this endpoint.
        /// </summary>
        protected override void CreatePendingConnection()
        {
            NetworkViewModel network = Parent?.Parent;
            if (network == null)
            {
                return;
            }

            PendingConnectionViewModel pendingConnection;
            if (MaxConnections == 1 && Connections.Items.Any())
            {
	            var conn = Connections.Items.First();
                pendingConnection = new PendingConnectionViewModel(network)
                {
                    Output = conn.Output,
                    OutputIsLocked = true,
                    LooseEndPoint = Port.CenterPoint
                };
                network.Connections.Remove(conn);
            }
            else if(Connections.Count < MaxConnections)
            {
                pendingConnection = new PendingConnectionViewModel(network) { Input = this, InputIsLocked = true, LooseEndPoint = Port.CenterPoint };
            }
            else
            {
                return;
            }

            pendingConnection.LooseEndPoint = Port.CenterPoint;
            network.PendingConnection = pendingConnection;
        }

        /// <summary>
        /// Sets this endpoint as the input of the pending connection and updates its validation.
        /// Called when the user drags and holds a pending connection over this endpoint.
        /// </summary>
        /// <param name="previewActive">
        /// True to set this endpoint as the output of the pending connection.
        /// To remove this endpoint from the pending connection, set this to false.
        /// </param>
        protected override void SetConnectionPreview(bool previewActive)
        {
            PendingConnectionViewModel pendingCon = Parent.Parent.PendingConnection;
            if (pendingCon.Input != null && (pendingCon.Input != this || pendingCon.InputIsLocked))
            {
                return;
            }

            if (previewActive)
            {
                pendingCon.Input = this;
                pendingCon.Validation = ConnectionValidator(pendingCon);
            }
            else
            {
                pendingCon.Input = null;
                pendingCon.Validation = null;
            }
        }

        /// <summary>
        /// Tries to create a new connection in the network based on the pending connection and this endpoint as the input.
        /// If the connection would be invalid, no connection is made.
        /// The pending connection is deleted.
        /// Called when the user drags and releases a pending connection over this endpoint.
        /// </summary>
        protected override void FinishPendingConnection()
        {
            NetworkViewModel network = Parent?.Parent;
            if (network == null)
            {
                return;
            }

            if (network.PendingConnection.Input == this && !network.PendingConnection.InputIsLocked)
            {
                //Only allow drag from output to input, not input to input
                if (network.PendingConnection.Input.Parent != network.PendingConnection.Output.Parent)
                {
                    //Dont allow connections between an input and an output on the same node
                    if (network.PendingConnection.Validation.IsValid)
                    {
                        //Don't allow a new connection if max amount of connections has been reached and we
                        //can't automatically remove one.
                        if (Connections.Count < MaxConnections || MaxConnections == 1)
                        {
                            //Connection is valid
	                        bool canCreateConnection = true;

                            if (MaxConnections == Connections.Count && MaxConnections == 1)
                            {
                                //Remove the connection to this input
                                network.Connections.Remove(Connections.Items.First());
                            }
							else if (MaxConnections > 2)
                            {
								// Make sure connection does not exist already.
	                            if (network.Connections.Items.Any(con => con.Output == network.PendingConnection.Output && con.Input == this))
	                            {
		                            canCreateConnection = false;
	                            }
							}

	                        if (canCreateConnection)
	                        {
		                        //Add new connection
		                        network.Connections.Add(network.ConnectionFactory(this, network.PendingConnection.Output));
							}
                        }
                    }
                }
            }
            network.RemovePendingConnection();
        }
    }
}
