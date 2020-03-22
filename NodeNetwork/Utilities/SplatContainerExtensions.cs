using System;
using System.Collections.Generic;
using System.Text;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace NodeNetwork.Utilities
{
    public static class SplatContainerExtensions
    {
        public static IMutableDependencyResolver UseNodeNetwork(this IMutableDependencyResolver container)
        {
            container.Register(() => new ConnectionView(), typeof(IViewFor<ConnectionViewModel>));
            container.Register(() => new ErrorMessageView(), typeof(IViewFor<ErrorMessageViewModel>));
            container.Register(() => new NetworkView(), typeof(IViewFor<NetworkViewModel>));
            container.Register(() => new NodeEndpointEditorView(), typeof(IViewFor<NodeEndpointEditorViewModel>));
            container.Register(() => new NodeInputView(), typeof(IViewFor<NodeInputViewModel>));
            container.Register(() => new NodeOutputView(), typeof(IViewFor<NodeOutputViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<NodeViewModel>));
            container.Register(() => new PendingConnectionView(), typeof(IViewFor<PendingConnectionViewModel>));
            container.Register(() => new PortView(), typeof(IViewFor<PortViewModel>));

            container.RegisterPlatformBitmapLoader();

            return container;
        }
    }
}
