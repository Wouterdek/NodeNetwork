using DynamicData;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// The factory method to create endpoint view models. Used in NodeViewModel.
    /// </summary>
    /// <param name="group">The endpoint group this view model wraps.</param>
    /// <param name="groupEndpoints">The endpoints that belong to this groups.</param>
    /// <param name="children">Nested endpoint groups.</param>
    /// <param name="endpointGroupViewModelFactory">The factory method used to create the nested endpoint group view models.</param>
    /// <returns>The view model for the endpoint group.</returns>
    public delegate EndpointGroupViewModel EndpointGroupViewModelFactory(
        EndpointGroup group,
        IObservableList<Endpoint> groupEndpoints,
        IObservableCache<Node<IGroup<Endpoint, EndpointGroup>, EndpointGroup>, EndpointGroup> children,
        EndpointGroupViewModelFactory endpointGroupViewModelFactory);
}
