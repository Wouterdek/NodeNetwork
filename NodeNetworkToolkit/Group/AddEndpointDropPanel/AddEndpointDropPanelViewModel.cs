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
        public ReactiveCommand<Unit, Unit> AddEndpointFromPendingConnection { get; }

        #region IsDropZoneVisible
        private ObservableAsPropertyHelper<bool> _isDropZoneVisible;
        public bool IsDropZoneVisible => _isDropZoneVisible.Value;
        #endregion

        #region GroupIOBinding
        public GroupIOBinding GroupIOBinding
        {
            get => _groupIOBinding;
            set => this.RaiseAndSetIfChanged(ref _groupIOBinding, value);
        }
        private GroupIOBinding _groupIOBinding;
        #endregion

        private bool isOnSubnetEntrance;
        private bool isOnSubnetExit;

        public AddEndpointDropPanelViewModel(bool isOnSubnetEntrance = false, bool isOnSubnetExit = false)
        {
            this.isOnSubnetEntrance = isOnSubnetEntrance;
            this.isOnSubnetExit = isOnSubnetExit;

            bool isOnSubnet = isOnSubnetEntrance || isOnSubnetExit;

            AddEndpointFromPendingConnection = ReactiveCommand.Create(() =>
            {
                var network = isOnSubnet ? GroupIOBinding.ExitNode.Parent : GroupIOBinding.GroupNode.Parent;
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
                this.WhenAnyValue(vm => vm.GroupIOBinding.ExitNode.Parent.PendingConnection)
                    .Select(CanCreateEndpointFromPendingConnection)
                    .ToProperty(this, vm => vm.IsDropZoneVisible, out _isDropZoneVisible);
            }
            else
            {
                this.WhenAnyValue(vm => vm.GroupIOBinding.GroupNode.Parent.PendingConnection)
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
