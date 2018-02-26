using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    [DataContract]
    public class OutputNodeViewModel : NodeViewModel
    {
        static OutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<OutputNodeViewModel>));
        }

        [DataMember]
        public ValueNodeInputViewModel<int?> ResultInput { get; }

        public OutputNodeViewModel()
        {
            Name = "Output";

            this.CanBeRemovedByUser = false;

            ResultInput = new ValueNodeInputViewModel<int?>
            {
                Name = "Value",
                Editor = new IntegerValueEditorViewModel()
            };
            this.Inputs.Add(ResultInput);
        }

        [JsonConstructor]
        [Obsolete("Serialization constructor only", true)]
        public OutputNodeViewModel(ValueNodeInputViewModel<int?> resultInput)
        {
            ResultInput = resultInput;
        }
    }
}
