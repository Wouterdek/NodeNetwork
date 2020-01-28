# Changelog

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
