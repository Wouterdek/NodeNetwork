# Cookbook: Creating custom node types

Creating custom node classes is a good way to encapsulate a reproducable node structure.
This example shows how to create a custom HelloWorld node class, give it a name and assign inputs and outputs.
Custom node classes can also be used to create nodes with a customized view, but this will be covered in another section.

## Creating the class

Create a new class and call it HelloWorldNode. Make it a subclass of NodeViewModel.
In the constructor, you can modify properties of the node as usual.

```
public class HelloWorldNode : NodeViewModel
{
    public HelloWorldNode()
    {
        this.Name = "Hello World Node";
    }
}
```

## Creating inputs and outputs

In the snippet below, an input and an output were added.
The input, labeled "Name", takes in a name as a string.
The output, labeled "Text", produces a string greeting the name from the input.
When the name on the input changes, the output value is automatically recalculated.

```
public class HelloWorldNode : NodeViewModel
{
	public ValueNodeInputViewModel<string> NameInput { get; }
    public ValueNodeOutputViewModel<string> TextOutput { get; }

    public HelloWorldNode()
    {
        this.Name = "Hello World Node";
        
        NameInput = new ValueNodeInputViewModel<string>()
        {
            Name = "Name"
        };
        this.Inputs.Add(NameInput);

        TextOutput = new ValueNodeOutputViewModel<string>()
        {
            Name = "Text",
            Value = this.WhenAnyObservable(vm => vm.NameInput.ValueChanged)
                .Select(name => $"Hello {name}!")
        };
        this.Outputs.Add(TextOutput);
    }
}
```

## Assigning a view

While the HelloWorldNode class is correct, adding an instance of it to a network will not work right now.
In order for the node to show up, ReactiveUI needs to know what view to create for this viewmodel.
To do this, add the following to the class:

```
static HelloWorldNode()
{
    Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<HelloWorldNode>));
}
```

This registers the default NodeView as the view to create for the HelloWorldNode viewmodel class.
This can also be used to assign custom views for your viewmodel.