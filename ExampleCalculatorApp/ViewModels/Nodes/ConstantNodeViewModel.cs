using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System.Runtime.Serialization;

namespace ExampleCalculatorApp.ViewModels.Nodes
{
    [DataContract]
    public class ConstantNodeViewModel : NodeViewModel
    {
        static ConstantNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<ConstantNodeViewModel>));
        }

        [DataMember] public IntegerValueEditorViewModel ValueEditor { get; set; } = new IntegerValueEditorViewModel();

        [DataMember] public ValueNodeOutputViewModel<int?> Output { get; set; }

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
