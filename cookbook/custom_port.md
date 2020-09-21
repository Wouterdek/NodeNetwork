# Custom ports

Custom ports can be useful for various reasons, such as to indicate different types of endpoints.
A fully implemented example can be found in [ExampleCodeGenApp](https://github.com/Wouterdek/NodeNetwork/blob/master/ExampleCodeGenApp/ViewModels/CodeGenPortViewModel.cs). You can also look at the default [`PortView`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Views.PortView.html) implementation ([cs](https://github.com/Wouterdek/NodeNetwork/blob/master/NodeNetwork/Views/PortView.cs), [xaml](https://github.com/Wouterdek/NodeNetwork/blob/master/NodeNetwork/Themes/PortView.xaml)) for more information.

Set the `Port` property to your own custom viewmodel.
```csharp
...
var input = new NodeInputViewModel
{
    Port = new CustomPortViewModel()
};
...
```

In your custom port viewmodel, register the view type that should be used. You can use the default `PortView()` or specify your own view.
```csharp
public class CustomPortViewModel : PortViewModel
{
    static CustomPortViewModel()
    {
        Splat.Locator.CurrentMutable.Register(() => new CustomPortView(), typeof(IViewFor<CustomPortViewModel>));
    }

    #region Size
    public double Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }
    private double _size;
    #endregion
}
```

This is a simple example of a custom port view.
In a more complete implementation you can use the visual states specified in `PortView` to change the look of the port depending on error/highlight/connection state.
Note that this example uses composition. It is also possible to have your view inherit from `PortView`, but this is less flexible.

```csharp
public partial class CustomPortView : IViewFor<CustomPortViewModel>
{
    #region ViewModel
    ...
    #endregion

    public CustomPortView()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
        	this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.PortView.ViewModel).DisposeWith(d);
        });
    }
}
```

```xaml
<UserControl x:Class="Example.CustomPortView"
             ...
             x:Name="Element">
    <views:PortView x:Name="PortView" RenderTransformOrigin="0.5,0.5">
        <views:PortView.Template>
            <ControlTemplate>
                <Ellipse Width="{Binding ViewModel.Size, ElementName=Element}"/>
            </ControlTemplate>
        </views:PortView.Template>
    </views:PortView>
</UserControl>
```
