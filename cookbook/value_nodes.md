# Cookbook: Passing values between nodes and defining output functions.

Most use-cases want to use the network to interactively produce values. In the Calculator example, the network is used to calculate integers. In the Shader editor example, the network produces fragment shader code. The CodeGen example produces LUA code.
In this section, the Hello world example (as shown in a previous section) will be modified to calculate strings and print them to console. To do this, we will be using the [NodeNetworkToolkit package](https://www.nuget.org/packages/NodeNetworkToolkit/) as it provides some essential helper classes. Make sure to add this package to your project from NuGet.

## Step 1: Reading values from an input

In your viewmodel definition, change 

`var node1Input = new NodeInputViewModel();`

to

`var node1Input = new ValueNodeInputViewModel<string>();`

The [`ValueNodeInputViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.ValueNode.ValueNodeInputViewModel-1.html) class keeps track of the any connected output and the value in its editor and provide you with the latest value. Its type argument, in this case `string`, is the type of value you want to receive from the input. It is highly recommended to use a nullable type here as default(T) will be used if no values are available or an error occurs. Connections with outputs that produce incompatible values will be automatically blocked. Note that `ValueNodeInputViewModel` only supports a single connection, because it can have only a single active value. The [`ValueListNodeInputViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.ValueNode.ValueListNodeInputViewModel-1.html) class can be used for inputs with multiple connections and keeps a list.

Consuming the value produced by the input can be done in two ways.
 
1. The first way is use the `Value` property on the input to read the current value.
2. The second and recommended way is to use the `ValueChanged` observable. [This page](http://reactivex.io/documentation/observable.html) provides more info about Observables and reactive programming in general. The `ValueChanged` observable will provide you with the latest value as soon as it has changed.

To print the input value each time it changes, add the following code.
```Csharp
node1Input.ValueChanged.Subscribe(newValue =>
    {
        Console.WriteLine(newValue);
    });
```

## Step 2: Producing values from an output

Similar to the input, change the following in your viewmodel definition

`var node2Output = new NodeOutputViewModel();`

to 

`var node2Output = new ValueNodeOutputViewModel<string>();`

The [`ValueNodeOutputViewModel`](https://wouterdek.github.io/NodeNetwork/api/api/NodeNetwork.Toolkit.ValueNode.ValueNodeOutputViewModel-1.html) class calculates values, stores them and passes them to inputs.
The type parameter indicates the type of the values that are produced by this output, here `string` to match the input.
In order for the output to work, it needs to have a value producing observable assigned.
In this simple example, we will provide a constant value of "Example string":

`node2Output.Value = Observable.Return("Example string");`

In more complicated setups, you can use Observables to watch changes in input values, calculate a value based on the changed input values and then automatically update the value of this output, notifing connected inputs.
This is demonstrated in later sections and in the examples in the repository.

