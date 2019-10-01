# Changelog

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
