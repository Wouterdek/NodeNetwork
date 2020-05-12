using System;
using System.Runtime.Serialization;
using ExampleShaderEditorApp.Model;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    [DataContract]
    public class ShaderNodeOutputViewModel : ValueNodeOutputViewModel<ShaderFunc>
    {
        static ShaderNodeOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<ShaderNodeOutputViewModel>));
        }

        [DataMember] public Type ReturnType { get; set; }
    }
}
