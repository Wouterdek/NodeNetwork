using System;
using System.Collections.Generic;
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

        public EndpointGroupViewModel(EndpointGroup group, IObservableList<NodeInputViewModel> visibleInputs, IObservableList<NodeOutputViewModel> visibleOutputs)
        {
            VisibleInputs = visibleInputs;
            VisibleOutputs = visibleOutputs;
            Group = group;
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

        #region Name

        /// <summary>
        /// The name of this group.
        /// </summary>
        public EndpointGroup Group { get; }

        #endregion

        protected bool Equals(EndpointGroupViewModel other)
        {
            return Equals(Group, other.Group);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((EndpointGroupViewModel)obj);
        }

        public override int GetHashCode()
        {
            return (Group != null ? Group.GetHashCode() : 0);
        }
    }
}
