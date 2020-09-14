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

        public AddEndpointDropPanelViewModel(bool isOnSubnetEntrance = false, bool isOnSubnetExit = false)
        {
            bool isOnSubnet = isOnSubnetEntrance || isOnSubnetExit;

            AddEndpointFromPendingConnection = ReactiveCommand.Create(() =>
            {
                var network = isOnSubnet ? GroupIOBinding.ExitNode.Parent : GroupIOBinding.GroupNode.Parent;
                var pendingConn = network.PendingConnection;

                NodeInputViewModel input;
                NodeOutputViewModel output;

                if (pendingConn.Input != null 
                    && pendingConn.Input.Parent != GroupIOBinding.GroupNode
                    && !(isOnSubnetEntrance && pendingConn.Input.Parent == GroupIOBinding.EntranceNode)
                    && !(isOnSubnetExit && pendingConn.Input.Parent == GroupIOBinding.ExitNode))
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
                else if (pendingConn.Output != null 
                         && pendingConn.Output.Parent != GroupIOBinding.GroupNode
                         && !(isOnSubnetEntrance && pendingConn.Output.Parent == GroupIOBinding.EntranceNode)
                         && !(isOnSubnetExit && pendingConn.Output.Parent == GroupIOBinding.ExitNode))
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
                else
                {
                    return;
                }

                network.Connections.Add(network.ConnectionFactory(input, output));
            });

            if (isOnSubnet)
            {
                this.WhenAnyValue(vm => vm.GroupIOBinding.ExitNode.Parent.PendingConnection)
                    .Select(conn => conn != null)
                    .ToProperty(this, vm => vm.IsDropZoneVisible, out _isDropZoneVisible);
            }
            else
            {
                this.WhenAnyValue(vm => vm.GroupIOBinding.GroupNode.Parent.PendingConnection)
                    .Select(conn => conn != null)
                    .ToProperty(this, vm => vm.IsDropZoneVisible, out _isDropZoneVisible);
            }
            
        }
    }
}
