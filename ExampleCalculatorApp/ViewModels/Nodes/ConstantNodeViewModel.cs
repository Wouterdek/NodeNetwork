using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    public class ConstantNodeViewModel : NodeViewModel
    {
        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();

        public ValueNodeOutputViewModel<int?> Output { get; }

        public ConstantNodeViewModel()
        {
            this.Name = "Constant";
            
            Output = new ValueNodeOutputViewModel<int?>
            {
                Name = "Value",
                Editor = ValueEditor,
                Value = this.WhenAnyValue(vm => vm.ValueEditor.Value)
            };
            this.Outputs.Add(Output);
        }
    }
}
