# Changelog

## 4.1.0
- Selected nodes now appear on top of other nodes
- Connecting a `ValueNodeOutputViewModel<IObservableList<T>>` to a `ValueListNodeInputViewModel<T>` will now append all items in the list from the output to the list of values in the input.

## 4.0.0

- Replaced all usages of ReactiveList with DynamicData as ReactiveList is obsolete. (issue #30)
- Fixed bug in ValueListNodeInput
- Fixed scheduling issue during async layouting
- Added list categories support (issue #25)
