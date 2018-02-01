# Cookbook: Validation

Validation is useful to check that the network built by the user is valid.
NodeNetwork supports two types of validation: connection validation and network validation.

## Connection validation

Connection validation is performed while the user is creating a connection to check whether or not the connection is allowed.
If the validation fails, the pending connection changes color to indicate that the connection cannot be made and creation of the connection is blocked.
Connection validation is configured through the `ConnectionValidator` property in the input viewmodel.
An example of this can be found in `ValueNodeInputViewModel`:

```Csharp
...
ConnectionValidator = pending => new ConnectionValidationResult(pending.Output is ValueNodeOutputViewModel<T>, null);
...
```

In this snippet, a lambda function is assigned to ConnectionValidator to block connections between incompatible datatypes. 
The function is called every time the user hovers over the input with a pending connection.
It accepts the pending connection as an argument and returns an instance of `ConnectionValidationResult`.
If the output on the other side of the pending connection is a ValueNodeOutputViewModel with the same datatype, then the connection is valid, otherwise it is invalid.
Optionally, a second parameter can be passed to the `ConnectionValidationResult` constructor. 
This is a viewmodel for a view, an error message, that is displayed when the connection is invalid.

## Network validation

Network validation is useful for performing validation on a larger, inter-node scale.
A good example of this can be found the Calculator example:

```Csharp
NetworkViewModel.Validator = network =>
{
    bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
    if (containsLoops)
    {
        return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
    }

    bool containsDivisionByZero = GraphAlgorithms.GetConnectedNodesBubbling(output)
        .OfType<DivisionNodeViewModel>()
        .Any(n => n.Input2.Value == 0);
    if (containsDivisionByZero)
    {
        return new NetworkValidationResult(false, true, new ErrorMessageViewModel("Network contains division by zero!"));
    }

    return new NetworkValidationResult(true, true, null);
};
```

The validation function that is assigned checks for loops and zero divisions.
Loops (sequences of connections that start and end with the same node) create circular dependencies and would produce infinite loops when trying to calculate values.
That is why the validator checks for loops and returns `new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"))` if it fails.
The first parameter specifies that the network is invalid. The second parameter specifies that the network should not be parsed as that might cause problems.
The last parameter is a viewmodel for an error message that is shown.

A division by zero is also an operation that causes problems in the network, and so the validator also checks for this.
Note that the second argument of the `NetworkValidationResult` is now `true`, indicating that the network is still traversable.
If a division by zero is present, the division output will produce null as an invalid default value. 
The network will produce null values, but traversing the network doesn't crash the program. 
In fact, if we were to indicate that traversal is not possible, this would break value propagation. A change in the connected output value would not be passed to the division input until the network validation succeeds because traversal is disabled, and so the error would not go away.

Also note that we used the network validator for the division by zero check and not the connection validator.
This is because the division by zero depends on the context of the network. 
A connected output could have the right type (integers) but the wrong value (zero) because of the inputs of the connected node.
Blocking the creation of the connection doesn't make sense here because the input value could change at any time after the creation of the connection.