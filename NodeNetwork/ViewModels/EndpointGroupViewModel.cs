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

        #region Group
        /// <summary>
        /// The endpoint group wrapping the name and the parent group of this group.
        /// </summary>
        public EndpointGroup Group { get; }
        #endregion

        #region Children
        /// <summary>
        /// The list of nested endpoint groups.
        /// </summary>
        public ReadOnlyObservableCollection<EndpointGroupViewModel> Children => _children;
        private readonly ReadOnlyObservableCollection<EndpointGroupViewModel> _children;
        #endregion

        public EndpointGroupViewModel(EndpointGroup group, IObservable<IChangeSet<Endpoint>> allEndpoints, IObservableCache<Node<EndpointGroup, EndpointGroup>, EndpointGroup> children, EndpointGroupViewModelFactory endpointGroupViewModelFactory)
        {
            Group = group;
            var groupEndpoints = allEndpoints.Filter(e => e.Group == group);
            VisibleInputs = groupEndpoints.Filter(e => e is NodeInputViewModel).Cast(e => (NodeInputViewModel)e).AsObservableList();
            VisibleOutputs = groupEndpoints.Filter(e => e is NodeOutputViewModel).Cast(e => (NodeOutputViewModel)e).AsObservableList();
            children
                .Connect()
                .Transform(n => endpointGroupViewModelFactory(n.Key, allEndpoints, n.Children, endpointGroupViewModelFactory))
                .Bind(out _children)
                .Subscribe();
        }
    }
}
