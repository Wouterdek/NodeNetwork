# Custom errors

As mentioned in the Validation chapter, both connection and network validators return a validation result object.
This object has a error viewmodel property that can be set in the constructor.
The built-in [`ErrorMessageViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.ViewModels.ErrorMessageViewModel.html) class can be used to display simple text-based error messages.

```csharp
...
return new ConnectionValidationResult(false, new ErrorMessageViewModel("Error message goes here"));
...
```

Alternatively, you can create a custom viewmodel class and use it instead.
The corresponding view will then be displayed when the network/connection is invalid.

```csharp
...
return new ConnectionValidationResult(false, new MyCustomErrorViewModel());
...
```
```csharp
public class MyCustomErrorViewModel : ReactiveObject
{
	static MyCustomErrorViewModel()
	{
		Splat.Locator.CurrentMutable.Register(() => new MyCustomErrorView(), typeof(IViewFor<MyCustomErrorViewModel>));
	}
	...
}
```
```csharp
public partial class MyCustomErrorView : IViewFor<MyCustomErrorViewModel>
{
	...
}
```

If you need even more flexibility with the error messages, you can create a custom network view.
