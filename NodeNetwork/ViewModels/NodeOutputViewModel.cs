using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Viewmodel class for outputs on a node.
    /// Outputs are endpoints that can only be connected to inputs.
    /// </summary>
    [DataContract]
    public class NodeOutputViewModel : Endpoint
    {
        static NodeOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NodeOutputViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public NodeOutputViewModel()
        {
            MaxConnections = Int32.MaxValue;
            this.PortPosition = PortPosition.Right;
        }

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

            PendingConnectionViewModel newCon = network.ConnectionFactory.CreatePendingConnection(network);
            newCon.Output = this;
            newCon.OutputIsLocked = true;
            newCon.LooseEndPoint = Port.CenterPoint;
            network.PendingConnection = newCon;
        }

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
                pendingCon.Validation = pendingCon.Input.ConnectionValidator.Validate(pendingCon);
            }
            else
            {
                pendingCon.Output = null;
                pendingCon.Validation = null;
            }
        }

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
                            if (!network.Connections.Any(con => con.Output == this && con.Input == network.PendingConnection.Input))
                            {
                                //Connection does not exist already
                                network.Connections.Add(network.ConnectionFactory.CreateConnection(network, network.PendingConnection.Input, this));
                            }
                        }
                    }
                }
            }

            network.RemovePendingConnection();
        }
    }
}
