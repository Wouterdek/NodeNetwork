# Changelog

## 5.2.0
- Added ListEntryBackgroundBrush, ListEntryBackgroundMouseOverBrush, ListEntryHandleBrush. (issue #104)
- Fixed bug where bad scheduling could produce inconsistent CurrentValues. (issue #108)
- Updated dependencies

## 5.1.2
- Updated dependencies

## 5.1.1
- Fixed endpoint group behavior on collapsing of the node, thanks to FabianNitsche (PR #96)
- Fixed WPF binding error 26 in SearchableContextMenuView.xaml, thanks to FabianNitsche (issue #98, PR #100)
- Fixed WPF binding error 4 related to SearchableContextMenuView in ExampleShaderEditorApp, thanks to FabianNitsche (issue #97, PR #99)

## 5.1.0
- Fix resizable property visibility (commit fbce9b7)
- Improvements to zooming system, thanks to danielcaceresm (PR #90)
- Make SetupLayoutEvent virtual to allow applications to change the PortViewModel.CenterPoint update system (PR #91)
- Added mouse gesture properties to dragcanvas and networkview to allow customization of the keybindings for dragging, cutting and selection. (see PR #93)
- Add SearchableContextMenu, AddNodeContextMenu, implemented in ExampleShaderEditor (see issue #94)

## 5.0.0
- BREAKING CHANGE: Moved registration of views in locator to NNViewRegistrar. Apps should now register the views on startup before using the library. An example can be found [here](https://github.com/Wouterdek/NodeNetwork/blob/5bfb345457139aa169feff5237b323b7dfec1407/ExampleCalculatorApp/App.xaml.cs#L17).
- Added node grouping/containers (issue #24)
- Added endpoint grouping, thanks to FabianNitsche (PR #69)
- Made nodes user-resizable.
- Endpoints are now sorted explicitly using the SortIndex property.
- Fixed scrolling issue in NodeListView, thanks to FabianNitsche (issue #76)
- NetworkViewModel.NetworkChanged now also triggers when node endpoints are added/removed (issue #79)
- Fixed issue where dragging a Thumb inside a node would also drag the node (issue #78)
- Fixed a couple of edge-case bugs

## 4.3.0
- Updated dependencies (issue #82)
- Added NetworkBackground property for backgrounds that move with the network (issue #61)
- Fixed nullpointerexception when WhenActivated is called before OnApplyTemplate in endpoints. (issue #71) 
- Added ZoomFactor to NetworkViewModel (issue #81)

## 4.2.0
- Updated dependencies (Now uses ReactiveUI 11.x)
- Added NodeMoveStart, NodeMove and NodeMoveEnd events to NetworkView (issue #56)
- Added support for .NET core 3.1, target framework version 4.7.2 instead of 4.7 (PR #26 and #55)
- Added support for icons and moving the editor next to the port. If no icon is set and the endpoint label is empty, the editor will move up. Check out commit 0e2d244 to see this being used in the examples. (issue #52)
- Added binding option to show and hide some of the NodeListView elements (PR #51)
- Replace OpenGL.NET with OpenTK in ExampleShaderEditorApp for .NET core compatibility

## 4.1.1
- Fixed bug where ports would be cut off on certain offsets (issue #41)
- Fixed 'Nodes move when using scroll bar' (issue #44)
- Fixed friction bug in layout engine
- Fixed bug where fixed nodes would break the force-directed layouter
- Updated dependencies (Now uses ReactiveUI 10.x), removed obsolete code

## 4.1.0
- Selected nodes now appear on top of other nodes
- Connecting a `ValueNodeOutputViewModel<IObservableList<T>>` to a `ValueListNodeInputViewModel<T>` will now append all items in the list from the output to the list of values in the input.

## 4.0.0

- Replaced all usages of ReactiveList with DynamicData as ReactiveList is obsolete. (issue #30)
- Fixed bug in ValueListNodeInput
- Fixed scheduling issue during async layouting
- Added list categories support (issue #25)
