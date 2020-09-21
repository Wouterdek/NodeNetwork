# Cookbook: Adding a node list

In most use-cases you want to let the user add new nodes to the network.
You can implement your own control that adds new nodes using [`NetworkViewModel.Nodes.Add()`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.ViewModels.NetworkViewModel.html#NodeNetwork_ViewModels_NetworkViewModel_Nodes), 
but [NodeNetworkToolkit](https://www.nuget.org/packages/NodeNetworkToolkit/) provides an easy-to-use list control that allows users to drag-and-drop new nodes in the network.
This example shows how to add it to a project.

## Creating the list viewmodel

In the viewmodel class of the control you want to add the list to, add a [`NodeListViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.NodeList.NodeListViewModel.html) property:

```csharp
public NodeListViewModel ListViewModel { get; } = new NodeListViewModel();
```

In the constructor, simply add factory methods for the [`NodeViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.ViewModels.NodeViewModel.html) subclasses you want add.

```csharp
ListViewModel.AddNodeType(() => new ExampleNodeViewModel());
```

This factory method will be called once to populate the list with examples of nodes, and then every time a new node of this type is added to the network.

## Add the list view

In the XAML for your GUI, add the [`NodeListView`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.NodeList.NodeListView.html) control:

```xaml
<nodeList:NodeListView x:Name="nodeList"/>
```

In the code-behind, bind the `ViewModel` property of the control to the correct viewmodel.

```csharp
this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel);
```
