﻿using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    [DataContract]
    public class SubtractionNodeViewModel : NodeViewModel
    {
        static SubtractionNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<SubtractionNodeViewModel>));
        }

        [DataMember] public ValueNodeInputViewModel<int?> Input1 { get; set; }
        [DataMember] public ValueNodeInputViewModel<int?> Input2 { get; set; }
        [DataMember] public ValueNodeOutputViewModel<int?> Output { get; set; }

        public SubtractionNodeViewModel()
        {
            Name = "Subtract";

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

            var subtract = this.WhenAnyValue(vm => vm.Input1.Value, vm => vm.Input2.Value)
                .Select(_ => Input1.Value != null && Input2.Value != null ? Input1.Value - Input2.Value : null);

            Output = new ValueNodeOutputViewModel<int?>
            {
                Name = "A - B",
                Value = subtract
            };
            Outputs.Add(Output);
        }
    }
}
