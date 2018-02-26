using System;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    [DataContract]
    public class SumNodeViewModel : NodeViewModel
    {
        static SumNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<SumNodeViewModel>));
        }

        [DataMember]
        public ValueNodeInputViewModel<int?> Input1 { get; }
        [DataMember]
        public ValueNodeInputViewModel<int?> Input2 { get; }
        [DataMember]
        public ValueNodeOutputViewModel<int?> Output { get; }

        public SumNodeViewModel()
        {
            Name = "Sum";

            Input1 = new ValueNodeInputViewModel<int?>
            {
                Name = "A",
                Editor = new IntegerValueEditorViewModel()
            };
            Inputs.Add(Input1);

            Input2 = new ValueNodeInputViewModel<int?>
            {
                Name = "B",
                Editor = new IntegerValueEditorViewModel()
            };
            Inputs.Add(Input2);

            Output = new ValueNodeOutputViewModel<int?>
            {
                Name = "A + B"
            };
            Outputs.Add(Output);

            SetupBindings();
        }

        [JsonConstructor]
        [Obsolete("Serialization constructor only", true)]
        public SumNodeViewModel(ValueNodeInputViewModel<int?> input1, ValueNodeInputViewModel<int?> input2, ValueNodeOutputViewModel<int?> output)
        {
            Input1 = input1;
            Input2 = input2;
            Output = output;

            SetupBindings();
        }

        private void SetupBindings()
        {
            Output.Value = this.WhenAnyValue(vm => vm.Input1.Value, vm => vm.Input2.Value)
                    .Select(_ => Input1.Value != null && Input2.Value != null ? Input1.Value + Input2.Value : null);
        }
    }
}
