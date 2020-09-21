using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.Group.AddEndpointDropPanel
{
    public class AddEndpointDropPanelViewModel : ReactiveObject
    {
        /// <summary>
        /// Take the pending connection from the super- or subnetwork, whichever is non-null,
        /// and add endpoints to GroupIOBinding that match this connection.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddEndpointFromPendingConnection { get; }

        #region IsDropZoneVisible
        public bool IsDropZoneVisible => _isDropZoneVisible.Value;
        private readonly ObservableAsPropertyHelper<bool> _isDropZoneVisible;
        #endregion

        #region GroupIOBinding
        public GroupIOBinding GroupIOBinding
        {
            get => _groupIOBinding;
            set => this.RaiseAndSetIfChanged(ref _groupIOBinding, value);
        }
        private GroupIOBinding _groupIOBinding;
        #endregion

        private readonly bool isOnSubnetEntrance;
        private readonly bool isOnSubnetExit;

        public AddEndpointDropPanelViewModel(bool isOnSubnetEntrance = false, bool isOnSubnetExit = false)
        {
            this.isOnSubnetEntrance = isOnSubnetEntrance;
            this.isOnSubnetExit = isOnSubnetExit;

            bool isOnSubnet = isOnSubnetEntrance || isOnSubnetExit;

            AddEndpointFromPendingConnection = ReactiveCommand.Create(() =>
            {
                var network = isOnSubnet ? GroupIOBinding.SubNetwork : GroupIOBinding.SuperNetwork;
                var pendingConn = network.PendingConnection;

                NodeInputViewModel input = null;
                NodeOutputViewModel output = null;

                if (!CanCreateEndpointFromPendingConnection(pendingConn))
                {
                    return;
                }

                if (pendingConn.Input != null)
                {
                    input = pendingConn.Input;
                    if (isOnSubnet)
                    {
                        output = GroupIOBinding.AddNewSubnetInlet(pendingConn.Input);
                    }
                    else
                    {
                        output = GroupIOBinding.AddNewGroupNodeOutput(pendingConn.Input);
                    }
                }
                else if (pendingConn.Output != null)
                {
                    if (isOnSubnet)
                    {
                        input = GroupIOBinding.AddNewSubnetOutlet(pendingConn.Output);
                    }
                    else
                    {
                        input = GroupIOBinding.AddNewGroupNodeInput(pendingConn.Output);
                    }
                    output = pendingConn.Output;
                }

                network.Connections.Add(network.ConnectionFactory(input, output));
            });

            if (isOnSubnet)
            {
                this.WhenAnyValue(vm => vm.GroupIOBinding.SubNetwork.PendingConnection)
                    .Select(CanCreateEndpointFromPendingConnection)
                    .ToProperty(this, vm => vm.IsDropZoneVisible, out _isDropZoneVisible);
            }
            else
            {
                this.WhenAnyValue(vm => vm.GroupIOBinding.SuperNetwork.PendingConnection)
                    .Select(CanCreateEndpointFromPendingConnection)
                    .ToProperty(this, vm => vm.IsDropZoneVisible, out _isDropZoneVisible);
            }
            
        }

        private bool CanCreateEndpointFromPendingConnection(PendingConnectionViewModel conn)
        {
            if (conn == null)
            {
                return false;
            }

            var sourceNode = conn.Input != null ? conn.Input.Parent : conn.Output.Parent;

            return sourceNode != GroupIOBinding.GroupNode 
                   && !(isOnSubnetEntrance && sourceNode == GroupIOBinding.EntranceNode)
                   && !(isOnSubnetExit && sourceNode == GroupIOBinding.ExitNode);
        }
    }
}
