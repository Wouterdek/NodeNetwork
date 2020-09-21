using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for outputs on a node.
    /// Outputs are endpoints that can only be connected to inputs.
    /// </summary>
    public class NodeOutputViewModel : Endpoint
    {
        static NodeOutputViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeOutputView(), typeof(IViewFor<NodeOutputViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public NodeOutputViewModel()
        {
            MaxConnections = Int32.MaxValue;
            this.PortPosition = PortPosition.Right;
        }

        /// <summary>
        /// Sets the pending connection in the network to a new connection with this endpoint as the output.
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

            if (Connections.Count >= MaxConnections)
            {
                return;
            }

            network.PendingConnection = new PendingConnectionViewModel(network) { Output = this, OutputIsLocked = true, LooseEndPoint = Port.CenterPoint };
        }

        /// <summary>
        /// Sets this endpoint as the output of the pending connection and updates its validation.
        /// Called when the user drags and holds a pending connection over this endpoint.
        /// </summary>
        /// <param name="previewActive">
        /// True to set this endpoint as the output of the pending connection.
        /// To remove this endpoint from the pending connection, set this to false.
        /// </param>
        protected override void SetConnectionPreview(bool previewActive)
        {
            PendingConnectionViewModel pendingCon = Parent.Parent.PendingConnection;
            if (pendingCon.Output != null && (pendingCon.Output != this || pendingCon.OutputIsLocked))
            {
                return;
            }

            if (previewActive)
            {
                pendingCon.Output = this;
                pendingCon.Validation = pendingCon.Input.ConnectionValidator(pendingCon);
            }
            else
            {
                pendingCon.Output = null;
                pendingCon.Validation = null;
            }
        }

        /// <summary>
        /// Tries to create a new connection in the network based on the pending connection and this endpoint as the output.
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

            if (network.PendingConnection.Output == this && !network.PendingConnection.OutputIsLocked)
            {
                //Only allow drag from output to input, not input to input
                if (network.PendingConnection.Input.Parent != network.PendingConnection.Output.Parent)
                {
                    //Dont allow connections between an input and an output on the same node
                    if (network.PendingConnection.Validation.IsValid)
                    {
                        //Connection is valid
                        if (MaxConnections > Connections.Count)
                        {
                            //MaxConnections hasn't been reached yet.
                            if (!network.Connections.Items.Any(con => con.Output == this && con.Input == network.PendingConnection.Input))
                            {
                                //Connection does not exist already
                                network.Connections.Add(network.ConnectionFactory(network.PendingConnection.Input, this));
                            }
                        }
                    }
                }
            }

            network.RemovePendingConnection();
        }
    }
}
