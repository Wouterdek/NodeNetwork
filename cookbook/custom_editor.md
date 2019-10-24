# Cookbook: Creating custom input editors

Input editors are useful to let the user configure values or properties for your node/input right where they are relevant.
Creating a custom editor involves two parts: a custom view and a custom viewmodel.

In this section, we will create an editor for strings. More complex examples can be found in the shader editor example app.
Also note that while this example uses [`ValueEditorViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.ValueNode.ValueEditorViewModel-1.html), you can also use [`NodeEndpointEditorViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.ViewModels.NodeEndpointEditorViewModel.html) if you want something more bare-bones.

## ViewModel

Create a new class and let it inherit from `ValueEditorViewModel<string>`.
All subclasses of `ValueEditorViewModel` have a `Value` property that is automatically used by `ValueInputEditorViewModel`.
In the constructor of the new class, assign a default value to the property.
In order to link the view to the viewmodel, you also need to register it with ReactiveUI.

The result looks like this:
```Csharp
public class StringValueEditorViewModel : ValueEditorViewModel<string>
{
    static StringValueEditorViewModel()
    {
        Splat.Locator.CurrentMutable.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
    }

    public StringValueEditorViewModel()
    {
        Value = "";
    }
}
```

## View

Create a new UserControl and add a textbox to the XAML:

```XAML
<TextBox x:Name="TextBox" MinWidth="100"/>
```

In the code-behind, have the control implement `IViewFor<StringValueEditorViewModel>` and bind the text in the textbox to the `Value` property in the ViewModel.

```Csharp
public partial class StringValueEditorView : IViewFor<StringValueEditorViewModel>
{
    #region ViewModel
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
        typeof(StringValueEditorViewModel), typeof(StringValueEditorView), new PropertyMetadata(null));

    public StringValueEditorViewModel ViewModel
    {
        get => (StringValueEditorViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StringValueEditorViewModel)value;
    }
    #endregion

    public StringValueEditorView()
    {
        InitializeComponent();

        this.WhenActivated(d => {
            this.Bind(ViewModel, vm => vm.Value, v => v.TextBox.Text).DisposeWith(d);
        });
    }
}
```
