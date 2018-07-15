# Cookbook: Adding a node list

In most use-cases you want to let the user add new nodes to the network.
You can implement your own control that adds new nodes using `NetworkViewModel.Nodes.Add()`, 
but NodeNetworkToolkit provides an easy-to-use list control that allows users to drag-and-drop new nodes in the network.
This example shows how to add it to a project.

## Creating the list viewmodel

In the viewmodel class of the control you want to add the list to, add a `NodeListViewModel` property:

```Csharp
public NodeListViewModel ListViewModel { get; } = new NodeListViewModel();
```

In the constructor, simply add factory methods for the NodeViewModel subclasses you want add.

```Csharp
ListViewModel.AddNodeType(() => new ExampleNodeViewModel());
```

This factory method will be called once to populate the list with examples of nodes, and then every time a new node of this type is added to the network.

## Add the list view

In the XAML for your GUI, add the `NodeListView` control:

```XAML
<nodeList:NodeListView x:Name="nodeList"/>
```

In the code-behind, bind the `ViewModel` property of the control to the correct viewmodel.

```
this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel);
```