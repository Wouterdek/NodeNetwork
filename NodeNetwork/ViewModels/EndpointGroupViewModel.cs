using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

using DynamicData;

using NodeNetwork.Views;

using ReactiveUI;

using Splat;

namespace NodeNetwork.ViewModels
{
    public class EndpointGroupViewModel : ReactiveObject
    {
        static EndpointGroupViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new EndpointGroupView(), typeof(IViewFor<EndpointGroupViewModel>));
        }

        public EndpointGroupViewModel(Node<IGroup<Endpoint, EndpointGroup>, EndpointGroup> node)
        {
            Group = node.Key;
            var endpoints = node.Item.List.Connect();
            VisibleInputs = endpoints.Filter(e => e is NodeInputViewModel).Transform(e => (NodeInputViewModel)e).AsObservableList();
            VisibleOutputs = endpoints.Filter(e => e is NodeOutputViewModel).Transform(e => (NodeOutputViewModel)e).AsObservableList();
            node.Children.Connect().Transform(n => new EndpointGroupViewModel(n)).Bind(out _children).Subscribe();
        }

        #region VisibleInputs
        /// <summary>
        /// The list of inputs that is currently visible on this node.
        /// Some inputs may be hidden if the node is collapsed.
        /// </summary>
        public IObservableList<NodeInputViewModel> VisibleInputs { get; }
        #endregion

        #region VisibleOutputs
        /// <summary>
        /// The list of outputs that is currently visible on this node.
        /// Some outputs may be hidden if the node is collapsed.
        /// </summary>
        public IObservableList<NodeOutputViewModel> VisibleOutputs { get; }
        #endregion


        /// <summary>
        /// The name of this group.
        /// </summary>
        public EndpointGroup Group { get; }

        private readonly ReadOnlyObservableCollection<EndpointGroupViewModel> _children;
        public ReadOnlyObservableCollection<EndpointGroupViewModel> Children => _children;

    }
}
