using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    public class OutputNodeViewModel : NodeViewModel
    {
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
    }
}
