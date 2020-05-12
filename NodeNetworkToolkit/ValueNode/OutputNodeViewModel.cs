using DynamicData;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System.Runtime.Serialization;

namespace NodeNetwork.Toolkit.ValueNode
{
    [DataContract]
    public abstract class OutputNodeViewModel<T> : NodeViewModel
    {
        static OutputNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<OutputNodeViewModel<T>>));
        }

        public OutputNodeViewModel(string name = "Call base() to set me", ValueEditorViewModel<T> editor = null, string resultName = "Value")
        {
            Name = name;

            CanBeRemovedByUser = false;

            Result = new ValueNodeInputViewModel<T>
            {
                Name = resultName,
                Editor = editor
            };

            Inputs.Add(Result);
        }

        [DataMember] public ValueNodeInputViewModel<T> Result { get; set; }
    }
}
